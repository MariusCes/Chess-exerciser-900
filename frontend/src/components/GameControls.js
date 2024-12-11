import React from 'react';

export const GameControls = ({
  move, 
  setMove, 
  postMove, 
  isGameCreated, 
  gameStatus,
  togglePieceVisibility
}) => {
  return (
    <>
      <form className="move-form">
        <input
          className="me-1"
          type="text"
          value={move}
          onChange={(e) => setMove(e.target.value)}
          placeholder="B1B2"
        />
        <button
          className="btn btn-secondary"
          type="submit"
          onClick={(e) => {
            e.preventDefault();
            postMove(move);
            setMove("");
          }}
          disabled={!isGameCreated || gameStatus !== null}
        >
          Submit Move
        </button>
      </form>

      <button
        onClick={togglePieceVisibility}
        className="btn btn-secondary mt-2"
      >
        Toggle Piece Visibility
      </button>
    </>
  );
};

export default GameControls;