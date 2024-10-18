import React, { useState, useRef, useEffect } from 'react'
import ReactCrop, { centerCrop, makeAspectCrop, convertToPixelCrop } from 'react-image-crop'
import 'react-image-crop/dist/ReactCrop.css'
import cx from './RxPhotoCrop.module.css'

/**
 * 應用情境：用手機照像並裁剪後上傳。
 * 規格SD:
 * 一、用 <input type=file /> 可在手機照像。把原圖送到 <ReactCrop /> 元件。
 * 二、用 <ReactCrop /> 元件取『裁剪座標』。
 * 三A、按下【裁剪】按鈕，操作 <canvas /> 元件依『裁剪座標』複製並縮圖。
 * 三B、手動操作另一個隱藏的 <input type=file /> 把縮圖手動上傳到後端主機。
 */

/// helper
function isMobileDevice() {
  return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
}

export default function RxPhotoCrop({ onImageCrop, onImageCropBefore, uploadInputId, maxHeight, maxWidth }) {
  const imgRef = useRef()
  const canvasCropRef = useRef()
  const takePhotoRef = useRef()

  const [isMobile] = useState(() => isMobileDevice())

  const [imgSrc, setImgSrc] = useState('')
  const [imgFileName, setImgFileName] = useState()

  const [crop, setCrop] = useState()
  const [completedCrop, setCompletedCrop] = useState()
  const [f_croping, setCroping] = useState(false)

  const [errMsg, setErrMsg] = useState()

  return (
    <div className={cx.wrapper}>
      <button className={cx.takePhotoButton} onClick={_ => takePhotoRef.current?.click()}>照像</button>
      <input className={cx.hiddenInput} ref={takePhotoRef} type='file' accept='image/*' capture='environment' onChange={handleSelectFile} />

      {/* 手機模式:在 overlay 上裁圖； 一般模式:在預設位置裁圖。 */}
      <div className={f_croping ? (isMobile ? cx.cropMobilePannel : null) : 'd-none'}>
        <ReactCrop crop={crop} onChange={(c) => setCrop(c)}
          className="d-block"
          circularCrop={false}
          keepSelection={true}
          onComplete={(c) => setCompletedCrop(c)}>
          <img src={imgSrc} ref={imgRef} onLoad={handleImageLoad} />
        </ReactCrop>

        <button className={cx.cropButton} onClick={handleCrop}>裁剪</button>
      </div>

      {errMsg && <div className={cx.errMsg}>{errMsg}</div>}

      <canvas className={cx.hiddenCanvas} ref={canvasCropRef} />

      {/* for debug
      <pre style={{ textAlign: 'left' }}>
        crop: {JSON.stringify(crop, null, '  ')}<br />
        completedCrop: {JSON.stringify(completedCrop, null, '  ')}<br />
      </pre> */}
    </div>
  )

  function handleSelectFile(e) {
    const file = e.target.files[0]
    if (!file) return

    // for debug
    //console.trace('handleSelectFile', file)

    const reader = new FileReader()
    reader.addEventListener('load', () => {
      const imageUrl = reader.result?.toString() || ''
      setImgSrc(imageUrl)
      setImgFileName(file.name)
      setCroping(true) // 載入照片後開始裁剪
    })

    reader.readAsDataURL(file)
  }

  function handleImageLoad(e) {
    const { width, height, naturalWidth, naturalHeight } = e.currentTarget

    // for debug
    //setImgInfo({ width, height, naturalWidth, naturalHeight })

    const aspect = naturalWidth / naturalHeight;
    const crop = centerCrop(makeAspectCrop({ unit: '%', width: 100 }, aspect, width, height), width, height);

    setCrop(crop)
    // 手動更新 completedCrop，因為重新照相後不會自動刷新。
    setCompletedCrop(convertToPixelCrop(crop, width, height))
  }

  // 裁剪與上傳
  async function handleCrop() {
    const image = imgRef.current

    //§ 三A、按下【裁剪】按鈕，操作 < canvas /> 元件依『裁剪座標』複製並縮圖。

    // 送出裁剪前訊息
    onImageCropBefore()

    //## 裁剪：複製裁剪區域再縮圖
    //# 計算裁剪比例
    const scaleX = image.naturalWidth / image.width
    const scaleY = image.naturalHeight / image.height

    //# 計算裁剪前座標長寬
    const naturalX = Math.ceil(completedCrop.x * scaleX)
    const naturalY = Math.ceil(completedCrop.y * scaleY)
    const naturalWidth = Math.floor(completedCrop.width * scaleX)
    const naturalHeight = Math.floor(completedCrop.height * scaleY)

    //# 計算縮圖長寬
    let width2 = naturalWidth
    let height2 = naturalHeight

    if (width2 > maxWidth) {
      height2 = Math.round(height2 * maxWidth / width2);
      width2 = maxWidth;
    }

    if (height2 > maxHeight) {
      width2 = Math.round(width2 * maxHeight / height2);
      height2 = maxHeight;
    }

    //※ iOS 尚不支援 OffscreenCanvas 只好用 canvas。
    //const canvasCrop = new OffscreenCanvas(
    //  completedCrop.width * scaleX,
    //  completedCrop.height * scaleY,
    //)

    //# 執行縮圖
    const canvasCrop = canvasCropRef.current;
    canvasCrop.width = width2;
    canvasCrop.height = height2

    const ctx = canvasCrop.getContext('2d')
    if (!ctx) {
      setErrMsg('The canvas is not support 2d context!')
      return
      //throw new Error()
    }

    ctx.drawImage(
      image,
      naturalX,
      naturalY,
      naturalWidth,
      naturalHeight,
      0,
      0,
      width2,
      height2,
    )

    //## 完成裁剪
    const imageType = 'image/png'
    //const imageDataUrl = canvasCrop.toDataURL(imageType);

    //------------------------------------------------
    //§ 三B、手動操作另一個隱藏的 <input type=file /> 把縮圖手動上傳到後端主機。
    //再間接透過 <input id='uploadInputId' type='file' /> 手動上傳圖片。

    // Step 1: Create Blob
    const blob = await canvasToBlob(canvasCrop, imageType);

    // Step 2: Create File
    const file = new File([blob], changeExtensionToPng(imgFileName), { type: imageType });

    // Step 3: Use DataTransfer to Set File to Input Element
    const dataTransfer = new DataTransfer();
    dataTransfer.items.add(file);

    const uploadElement = document.getElementById(uploadInputId);
    uploadElement.files = dataTransfer.files;

    // Step 4:  模擬點擊上傳
    uploadElement.dispatchEvent(new Event('change', { bubbles: true }));

    //## 完成全部程序送出訊息

    // for debug
    const imageCropInfo = {
      imageFileName: `${imgFileName}.png`,
      imageType: imageType,
      blobSize: blob.size,
      width: width2,
      height: height2
    }

    // for debug
    //setImg2Info(imageCropInfo)

    // 送出完成訊息
    onImageCrop(imageCropInfo)

    // 完成裁剪
    setCroping(false)
  }

  /// helper:包裝器函數，將 canvas.toBlob 包裝為 Promise。
  function canvasToBlob(canvas, type = 'image/png', quality = 1.0) {
    return new Promise((resolve, reject) => {
      canvas.toBlob(blob => {
        if (blob) {
          resolve(blob);
        } else {
          reject(new Error('Canvas to Blob conversion failed'));
        }
      }, type, quality);
    });
  }

  /// helper: 變更檔案副檔名成 png。
  function changeExtensionToPng(filename) {
    // 使用正則表達式匹配檔案名稱和副檔名
    const regex = /^(.+?)(\.[^.]*$|$)/;

    // 使用 replace 方法，保留檔案名稱，但將副檔名替換為 .png
    return filename.replace(regex, '$1.png');
  }
}
