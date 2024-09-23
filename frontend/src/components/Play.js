import React, { useState } from 'react';
import '../styles/Play.css';

function Play() {
  const [move, setMove] = useState('');

  return (
    <div className="play-screen">
      <img src={"/chess.jpg"} alt="Chess board" className="chess-board-image" />
      <form className="move-form">
        <input 
          type="text" 
          value={move}
          onChange={(e) => setMove(e.target.value)}
          placeholder="B1 to B2"
        />
        <button type="submit">Submit Move</button>
      </form>
    </div>
  );
}

export default Play;
