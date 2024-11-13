import React from 'react';
import '../styles/Board.css';

const pieceImages = {
  'P': '/pieces/white-pawn.png',
  'R': '/pieces/white-rook.png',
  'N': '/pieces/white-knight.png',
  'B': '/pieces/white-bishop.png',
  'Q': '/pieces/white-queen.png',
  'K': '/pieces/white-king.png',
  'p': '/pieces/black-pawn.png',
  'r': '/pieces/black-rook.png',
  'n': '/pieces/black-knight.png',
  'b': '/pieces/black-bishop.png',
  'q': '/pieces/black-queen.png',
  'k': '/pieces/black-king.png'
};

const parseFEN = (fen) => {
  const rows = fen.split(' ')[0].split('/');
  return rows.map(row => {
    const parsedRow = [];
    for (const char of row) {
      if (!isNaN(char)) {
        parsedRow.push(...Array(parseInt(char)).fill(null));
      } else {
        parsedRow.push(char);
      }
    }
    return parsedRow;
  });
};

const Board = ({ fen, turnBlack }) => {
  const boardArray = parseFEN(fen);
  const columnLabels = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'];
  const rowLabels = ['8', '7', '6', '5', '4', '3', '2', '1'];

  return (
    <div className="board-container">
      <div className="column-labels top-labels">
        {columnLabels.map((label, index) => (
          <span key={`col-top-${index}`} className="column-label">{label}</span>
        ))}
      </div>

      <div className="board-wrapper">
        <div className="row-labels">
          {rowLabels.map((label, index) => (
            <span key={`row-${index}`} className="row-label">{label}</span>
          ))}
        </div>

        <div className="board">
          {boardArray.map((row, rowIndex) =>
            row.map((piece, colIndex) => {
              const isDarkSquare = (rowIndex + colIndex) % 2 === 1;
              return (
                <div
                  className={`square ${isDarkSquare ? 'dark-square' : 'light-square'}`}
                  key={`${rowIndex}-${colIndex}`}
                > {piece && (
                  <img
                    src={pieceImages[piece]} alt={piece} className={`piece ${turnBlack ? 'hidden' : ''}`}
                  />
                )}
                </div>
              );
            })
          )}
        </div>
      </div>

      <div className="column-labels bottom-labels">
        {columnLabels.map((label, index) => (
          <span key={`col-bottom-${index}`} className="column-label">{label}</span>
        ))}
      </div>
    </div>
  );
};

export default Board;
