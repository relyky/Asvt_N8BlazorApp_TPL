import React, { useEffect, useId } from 'react'
import { Html5QrcodeScanner, Html5QrcodeSupportedFormats } from "html5-qrcode"

export default function RxQRCodeScanner({ onScanSuccess, onScanFailure }) {
  const readerId = useId()

  //function onScanSuccess(decodedText, decodedResult) {
  //  // handle the scanned code as you like, for example:
  //  console.log(`Code matched => ${decodedText}`, decodedResult);
  //}

  //function onScanFailure(error) {
  //  // handle scan failure, usually better to ignore and keep scanning.
  //  // for example:
  //  console.warn(`Code scan error => ${error}`);
  //}

  useEffect(() => {
    const html5QrcodeScanner = new Html5QrcodeScanner(
      readerId,
      {
        fps: 10,
        qrbox: { width: 250, height: 250 },
        formatsToSupport: [Html5QrcodeSupportedFormats.QR_CODE], // 限制只掃 QR Code。 好像無效?
        videoConstraints: {
          facingMode: "environment", // 環境境頭優先
        }
      },
      /* verbose= */ false);

    html5QrcodeScanner.render(onScanSuccess, onScanFailure);
  }, []);

  return (
    <div id={readerId}></div>
  )
}
