import React, { useEffect, useId } from 'react'

function isMobileDevice() {
  return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
}

//§ 機制：鎖定過期螢幕。同一時間只有最近打開的螢幕有效。
export default function WebBlackScreen({ idname /* string */ }) {
  const uid = useId();

  useEffect(() => {
    const loadtimeId = (typeof idname === 'string') ? idname : 'loadtime'
    const blackscreenId = 'blackscreen-'.concat(uid)

    if (isMobileDevice()) return;
    // 行動平台無效！

    function handleLoad(event) {
      const now = Date.now();
      sessionStorage.setItem(loadtimeId, now);
      localStorage.setItem(loadtimeId, now);
    }

    function handleFocus(event) {
      if (!document.getElementById(blackscreenId)) {
        const sessionLoadTime = sessionStorage.getItem(loadtimeId);
        const localLoadTime = localStorage.getItem(loadtimeId);
        if (sessionLoadTime < localLoadTime) {
          const blackscreen = document.createElement('div');
          blackscreen.style = "position:fixed;display:block;width:100%;height:100%;top:0;left:0;right:0;bottom:0;background-color:rgba(0,0,0,0.5);z-index:99999;cursor:pointer;";
          blackscreen.innerHTML = "<H1 style='color:red;text-align:center;'>此介面已鎖住</H1><H2 style='color:red;text-align:center;'>同一時間相同系統介面只有最新的有效。</H2>";
          blackscreen.id = blackscreenId;
          document.body.appendChild(blackscreen);
        }
      }
    }

    window.addEventListener('load', handleLoad)
    window.addEventListener('focus', handleFocus)
    return () => {
      window.removeEventListener('load', handleLoad)
      window.removeEventListener('focus', handleFocus)
    }
  }, []);

  return (
    <></>
  )
}
