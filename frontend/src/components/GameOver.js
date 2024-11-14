import React from "react";
import "../styles/GameOver.css";

const GameOver = ({ status, moveList, onClose }) => {
  const isWin = status === 'win';

  return (
    <div className="popup-overlay">
      <div className="popup-content">
        <div className="content-wrapper">
          <h1 className={`game-status ${isWin ? 'win' : 'loss'}`}>
            {isWin ? 'Victory!' : 'Game Over'}
          </h1>

          <div className="move-summary">
            <p className="move-count">
              Total Moves: <span className="move-count-number">{moveList.length/2}</span>
            </p>
          </div>

          <div className="move-history-container">
            <h2 className="history-title">Move History</h2>
            <div className="move-history">
              <ul>
                {moveList.map((move, index) => (
                  <li key={index} className={`move-item ${index % 2 === 0 ? 'even' : 'odd'}`}>
                    <span className="move-label">
                      {`Move ${Math.floor(index / 2 + 1)}${index % 2 === 0 ? 'a' : 'b'}`}
                    </span>
                    <span className="move-text">
                      {move}
                    </span>
                  </li>
                ))}
              </ul>
            </div>
          </div>

          <button onClick={onClose} className="play-again-button">
            Play Again
          </button>
        </div>
      </div>
    </div>
  );
};

export default GameOver;
