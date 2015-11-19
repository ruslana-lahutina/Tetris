﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.MVC.Model;
using DG.Tweening;

namespace Assets.MVC.View
{
    public class TetrisView
    {
        private GameObject _playersBoard;

        private const float X_CORRECTION = 2.25f;
        private const float Y_CORRECTION = 5.25f;
        private readonly static Vector2 NextShapePosition = new Vector2(3.75f, 3.75f);
        private Vector3 _boardsOffset;

        private GameObject _currentShape;
        private GameObject _nextShape;
        private List<Transform> _animationObjects; 
        
        private readonly List<GameObject> _lines; 
        private readonly TetrisModel _model;
        private readonly Controller.Controller _controller;

        public TetrisView(TetrisModel model, Controller.Controller controller)
        {
            _model = model;
            _controller = controller;

            _lines = new List<GameObject>();

            _model.MovementDone += OnMovementDone;
            _model.RotateDone += OnRotateDone;
            _model.ShapeAdded += OnShapeAdded;
            _model.LineDestroyed += OnLineDestroyed;
            _model.GameOver += OnGameOver;
            _model.ShapeDropping += OnShapeDropping;
            _model.ShapeIsAttachedAddListener(OnShapeIsAttached);
            _model.BlockIsAttachedAddListener(OnAttachedBlock);
        }

        public void NewGame(Vector3 boardsOffset)
        {
            _boardsOffset = boardsOffset;
            DisplayBoard();
            DisplayNextShape(_model.NextShape);
            SpawnShape(_model.CurrentShape, _model.CurrentShapeCoord);
        }

        public void DisplayBoard()
        {
            _playersBoard = new GameObject("Player");
            _playersBoard.transform.position = _boardsOffset;

            var board = UnityEngine.Object.Instantiate((GameObject)Resources.Load("Board"));
            board.transform.SetParent(_playersBoard.transform, false);

            for (var i = 0; i < _model.BoardHeight; i++)
            {
                _lines.Add(new GameObject("Line " + i));
                _lines[i].transform.SetParent(board.transform, false);
                _lines[i].transform.position += _lines[i].transform.up*(ShapeFactory.BLOCK_SIZE*i - Y_CORRECTION);
            }
        }

        public void SpawnShape(TetrisShape shape, Point spawnCoord)
        {
            _currentShape = ShapeFactory.CreateShape(shape);
            _currentShape.transform.position = new Vector2(
                spawnCoord.X * ShapeFactory.BLOCK_SIZE - X_CORRECTION, 
                spawnCoord.Y * ShapeFactory.BLOCK_SIZE - Y_CORRECTION);
            _currentShape.transform.SetParent(_playersBoard.transform, false);
        }

        public void PaintShape(Color color)
        {
            foreach (Transform block in _currentShape.transform)
            {
                block.GetComponent<SpriteRenderer>().color = color;
            }
        }

        public void RotateShape(TetrisShape shape)
        {
            Vector2 currentShapePosition = _currentShape.transform.position;

            UnityEngine.Object.Destroy(_currentShape);

            _currentShape = ShapeFactory.CreateShape(shape);
            _currentShape.transform.position = currentShapePosition;
            _currentShape.transform.SetParent(_playersBoard.transform);
        }

        private void DestroyLine(int number)
        {
            foreach (Transform block in _lines[number].transform)
            {
                UnityEngine.Object.Destroy(block.gameObject);
            }
        }

        private void DropBlocks(int startBlockIndex)
        {
            _animationObjects = new List<Transform>();
            _controller.DroppingBlocksAnimationStart();

            for (var i = startBlockIndex + 1; i < _model.BoardHeight; i++)
            {
                while (_lines[i].transform.childCount > 0)
                {
                    var block = _lines[i].transform.GetChild(0);
                    block.SetParent(_lines[i - 1].transform, true);

                    _animationObjects.Add(block);

                    block.DOMoveY(block.transform.position.y - ShapeFactory.BLOCK_SIZE, 0.25f)
                        .OnComplete(() => AnimationCompleted(block));
                }
            }
        }

        private void AnimationCompleted(UnityEngine.Object animatedObject)
        {
            if (_animationObjects.Last() == animatedObject) _controller.DroppingBlocksAnimationEnded();
        }

        public void DisplayNextShape(TetrisShape shape)
        {
            UnityEngine.Object.Destroy(_nextShape);
            _nextShape = ShapeFactory.CreateShape(shape);
            _nextShape.transform.position = NextShapePosition;
            _nextShape.transform.SetParent(_playersBoard.transform, false);
        }

        public void DisplayGameOver()
        {
        }

        private void OnAttachedBlock(object sender, int x, int y)
        {
            _currentShape.transform.GetChild(0).transform.parent = _lines[y].transform;
        }

        private void OnMovementDone(object sender, MovementEventArgs e)
        {
            _currentShape.transform.position += new Vector3(e.MoveVector.X, e.MoveVector.Y) * ShapeFactory.BLOCK_SIZE;
        }

        private void OnRotateDone(object sender, EventArgs e)
        {
            RotateShape(_model.CurrentShape);
        }

        private void OnShapeAdded(object sender, EventArgs e)
        {
            DisplayNextShape(_model.NextShape);
            SpawnShape(_model.CurrentShape, _model.CurrentShapeCoord);
        }

        private void OnShapeIsAttached(object sender, EventArgs e)
        {
            UnityEngine.Object.Destroy(_currentShape);
        }

        private void OnLineDestroyed(object sender, LineIndexEventArgs e)
        {
            DestroyLine(e.LineIndex);
            DropBlocks(e.LineIndex);
        }

        private void OnGameOver(object sender, EventArgs e)
        {
            Debug.Log("Game Over!");
            Application.LoadLevel("MainMenu");
        }

        private void OnShapeDropping(object sender, DroppingEventArgs e)
        {
            _controller.DroppingShapeAnimationStart();
            //UnityEngine.Debug.Log("Start animation of dropping shape, distance: " + e.Distance);
            _currentShape.transform.DOMoveY(_currentShape.transform.position.y - ShapeFactory.BLOCK_SIZE*e.Distance, 0.2f)
                .OnComplete(_controller.DroppingShapeAnimationEnded);
        }
    }

}
