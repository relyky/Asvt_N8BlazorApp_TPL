import React from 'react'
//import { ErrorBoundary } from "react-error-boundary"
//import ReactPlayer from 'react-player'
import YouTubePlayer from 'react-player/youtube'

/// YouTube media player
export default function WebYouTubePlayer({
  url /* string */,
  playing  /* boolean */,
  loop     /* boolean */,
  controls /* boolean */
}) {
  return (
    <div>
      <h6 style={{ fontSize: '1.2rem' }}>YouTube Player</h6>
      <pre>
        {`url: ${url}`}<br />
        {`playing: ${playing} | loop: ${loop} | controls: ${controls}`}
      </pre>

      <YouTubePlayer url={url} playing={playing} loop={loop} controls={controls}
        fallback={<div style={{ color: 'red' }}>YouTubePlayer 掛了</div>} />

    </div>
  )
}

