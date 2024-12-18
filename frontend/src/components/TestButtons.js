import React from 'react';

export const TestButtons = ({
  setGameStatus, 
  decreaseHealth, 
  mockCreateGame, 
  resetGame,
  developerMode,
  togglePieceVisibility
}) => {

  if (!developerMode) return null;
  
  return (
    <>
      <div className="test-buttons mt-3">
        <button
          onClick={() => setGameStatus("win")}
          className="btn btn-success me-2"
        >
          Test Win
        </button>
        <button
          onClick={() => setGameStatus("draw")}
          className="btn btn-warning me-2"
        >
          Test Draw
        </button>
        <button
          onClick={() => setGameStatus("lose")}
          className="btn btn-danger"
        >
          Test Lose
        </button>

        <button
          onClick={() => decreaseHealth(10)}
          className="btn btn-warning ms-2"
        >
          Decrease Health
        </button>
      </div>
      <div className="test-buttons2 mt-3">
        <button onClick={mockCreateGame} className="btn btn-success me-2">
          Mock create game
        </button>
        <button onClick={resetGame} className="btn btn-danger">Reset Game</button>
      </div>
      <button onClick={togglePieceVisibility} className="btn btn-secondary">
          Toggle Piece Visibility
        </button>
    </>
  );
};

export default TestButtons;