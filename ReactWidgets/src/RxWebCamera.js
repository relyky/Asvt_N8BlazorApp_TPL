import React, { useState } from 'react'
import Webcam from "react-webcam"

const containerStyle = {
  width: '100%',
  height: '100%',
  maxWidth: 640,
  maxHeight: 480,
  margin: '0 auto',
  border: 'solid 2px red',
  borderRadius: 12,
  overflow: 'hidden',
  position: 'relative'
}

const webcamStyle = {
  width: '100%',
  height: 'auto',
  maxWidth: '100%',
  maxHeight: '100%',
  objectFit: 'contain',
};

const btnPrimary = {
  color: '#fff',
  backgroundColor: '#0d6efd',
  padding: 4,
  borderRadius: 4,
  width: 100,
  position: 'absolute',
  bottom: 8,
  left: 'calc(50% - 50px)'
}

const videoConstraints = {
  facingMode: {
    ideal: "environment" // 優先選取環境鏡頭
  }
}

export default function RxWebCamera({ screenshotFormat /* string */, onCapture /* event */ }) {
  const [imageB64, setImageB64] = useState('')

  return (
    <>
      <div style={containerStyle}>
        <Webcam
          style={webcamStyle}
          screenshotFormat={screenshotFormat}
          videoConstraints={videoConstraints}
        >
          {({ getScreenshot }) => (
            <button style={btnPrimary} onClick={() => {
              const imageSrc = getScreenshot()
              setImageB64(imageSrc)
              console.trace('getScreenshot =>', { imageSrc })
              onCapture(imageSrc)
            }}>
              Capture photo
            </button>
          )}
        </Webcam>
      </div>

      {/*for debug*/}
      <div>
        <p style={{ color: 'red' }}>WebCam 比較適合用在 Web 不能將手機照像特性全開。</p>
        <img src={imageB64} />
      </div>
    </>
    
  )
}
