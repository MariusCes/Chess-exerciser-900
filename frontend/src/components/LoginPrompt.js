import React from "react";
import "../styles/GameOver.css";

const LoginPrompt = ({ onClose }) => {
  return (
    <div className="popup-overlay">
      <div className="popup-content">
        <button onClick={onClose} className="close-button">
          ✕
        </button>
        <div className="content-wrapper">
          <h1 className="game-status loss">Login Required</h1>
          
          <div className="login-message">
            <p>JOIN THE MASTERS TO PLAY</p>
          </div>

          <button onClick={onClose} className="play-again-button">
            Okay
          </button>
        </div>
      </div>
    </div>
  );
};

export default LoginPrompt;