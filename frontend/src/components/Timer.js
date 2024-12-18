import React from 'react';

export const Timer = ({ seconds }) => {
  const formatTimer = (totalSeconds) => {
    const minutes = Math.floor(totalSeconds / 60);
    const secs = totalSeconds % 60;
    return `${minutes.toString().padStart(2, "0")}:${secs
      .toString()
      .padStart(2, "0")}`;
  };

  return (
    <div className="timer-container mt-2">
      <span className="timer">{formatTimer(seconds)}</span>
    </div>
  );
};

export default Timer;