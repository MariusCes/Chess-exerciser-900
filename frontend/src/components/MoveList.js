import React from 'react';

export const MoveList = ({ moves }) => {
  return (
    <div className="move-list-container">
      <ul className="move-list">
        {moves.map((move, index) => (
          <li
            key={index}
            className={`move-item ${
              index % 2 === 0 ? "your-move" : "bot-move"
            }`}
          >
            {move}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default MoveList;