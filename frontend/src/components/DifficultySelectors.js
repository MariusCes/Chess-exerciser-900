import React from 'react';

export const DifficultySelectors = ({
  aiDifficulty, 
  setAiDifficulty, 
  memoryDifficulty, 
  setMemoryDifficulty,
  createGame,
  setHealth
}) => {
  return (
    <div className="d-flex align-items-center justify-content-center mb-3">
      <select
        className="form-select me-2"
        value={aiDifficulty}
        onChange={(e) => setAiDifficulty(e.target.value)}
        style={{ width: "120px" }}
      >
        <option value="">Select AI</option>
        <option value="1">Baby</option>
        <option value="2">Kid</option>
        <option value="3">Casual</option>
        <option value="4">Average MIF student</option>
        <option value="5">Competitive player</option>
        <option value="6">Professional player</option>
        <option value="7">Drunk Magnus Carlsen</option>
        <option value="8">AI overlord</option>
      </select>

      <select
        className="form-select me-2"
        value={memoryDifficulty}
        onChange={(e) => {
          const value = e.target.value;
          setMemoryDifficulty(value);

          switch (value) {
            case "1":
              setHealth(100);  // Easy
              break;
            case "2":
              setHealth(80);   // Medium
              break;
            case "3":
              setHealth(60);   // Hard
              break;
            default:
              setHealth(0);
              break;
          }
        }}
        style={{ width: "160px" }}
      >
        <option value="">Select difficulty</option>
        <option value="1">easy</option>
        <option value="2">medium</option>
        <option value="3">hard</option>
      </select>

      <button
        className="btn btn-secondary"
        onClick={createGame}
        disabled={!aiDifficulty || !memoryDifficulty}
      >
        Create Game
      </button>
    </div>
  );
};

export default DifficultySelectors;