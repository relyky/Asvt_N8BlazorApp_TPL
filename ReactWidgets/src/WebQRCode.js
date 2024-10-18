import React, { useEffect, useMemo, useState } from 'react'
import { QRCodeCanvas } from '@akamfoad/qrcode'

//§ 機制：WebComponent 與 Blazor 通訊
//※ web-component 的 props 名稱需全小寫，否則通訊會失敗。

export default function WebQRCode({ value /* string */, level /* [L,M*,Q,H] */, size /* number */, image_url /* string */, fg_color /* string */, bg_color /* string */ }) {
  const [dataUrlWithQRCode, setDataUrlWithQRCode] = useState(null)

  const config = useMemo(() => ({
    level: level || 'M',
    fgColor: fg_color || '#000',
    bgColor: bg_color || '#FFF',
    image: image_url && {
      source: image_url,
      width: '20%',
      height: '20%',
      x: 'center',
      y: 'center',
    },
  }), [level, image_url, fg_color, bg_color])

  useEffect(() => {
    async function calcDataUrlAsync() {
      const qrCanvas = new QRCodeCanvas(value, config)
      const dataUrl = await qrCanvas.toDataUrl()
      setDataUrlWithQRCode(dataUrl)
    }

    calcDataUrlAsync()
  }, [value, config])

  /* for debug
  console.trace('WebQRCode.render', { value, level, size, image_url, bg_color, fg_color }) */
  return (
    <div role='qrcode' style={{ display: 'inline-flex', width: size }}>
      {dataUrlWithQRCode && <img style={{ width: '100%' }} src={dataUrlWithQRCode} />}
    </div>
  )
}
