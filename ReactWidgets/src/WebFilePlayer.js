import React from 'react'
//import { ErrorBoundary } from "react-error-boundary"
//import ReactPlayer from 'react-player'
import FilePlayer from 'react-player/file'

/// RTSP/HLS media player
export default function WebFilePlayer({
  url /* string */,
  playing  /* boolean */,
  loop     /* boolean */,
  controls /* boolean */
}) {
  return (
    <div>
      <h6 style={{ fontSize: '1.2rem' }}>File Player</h6>
      <pre>
        {`url: ${url}`}<br />
        {`playing: ${playing} | loop: ${loop} | controls: ${controls}`}
      </pre>

      <FilePlayer url={url} playing={playing} loop={loop} controls={controls}
        fallback={<div style={{ color: 'red' }}>FilePlayer 掛了</div>} />

    </div>
  )
}



