import React, { useEffect, useMemo, useRef } from 'react'
import { QRCodeSVG } from '@akamfoad/qrcode'

//§ 機制：WebComponent 與 Blazor 通訊
//※ web-component 的 props 名稱需全小寫，否則通訊會失敗。

export default function WebQRCodeSVG({ value /* string */, level /* [L,M*,Q,H] */, size /* number */, image_url /* string */, fg_color /* string */, bg_color /* string */ }) {
  const divSvgRef = useRef()

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
    const qrSVG = new QRCodeSVG(value, config)
    const xmlWithQRCode = qrSVG.toString()
    divSvgRef.current.innerHTML = xmlWithQRCode
    /* for debug
    console.trace('WebQRCodeSVG.useEffect_AFTER', { innerHTML: divSvgRef.current.innerHTML }) */
  }, [value, config])

  /* for debug
  console.trace('WebQRCodeSVG.render', { value, level, size, image_url, bg_color, fg_color }) */
  return (
    <div role='qrcode' ref={divSvgRef} style={{ display: 'flex', width: size}}></div>
  )
}
