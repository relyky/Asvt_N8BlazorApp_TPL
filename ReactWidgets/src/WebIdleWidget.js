import React, { useEffect, useId, useState } from 'react'

//§ 機制：IDLE 倒數器。使用者有段時間，如5分鐘，沒操作系統就自動登出。
export default function WebIdleWidget({ timeout /* number */, signout /* uri */ }) {
  const [idleTimer, setIdleTimer] = useState(timeout || 100)
  // 5分鐘：100次 x 3秒 = 300秒 = 5分鐘。

  // count-down timer
  useEffect(() => {
    document.addEventListener('click', resetIdleIimer);
    document.addEventListener('mousemove', resetIdleIimer);
    document.addEventListener('mouseenter', resetIdleIimer);
    document.addEventListener('keydown', resetIdleIimer);
    document.addEventListener('scroll', resetIdleIimer);
    document.addEventListener('touchstart', resetIdleIimer);
    const intervalId = setInterval(decreaseIdleTimer, 3000); // 每3秒倒數一次
    return () => {
      document.removeEventListener('click', resetIdleIimer);
      document.removeEventListener('mousemove', resetIdleIimer);
      document.removeEventListener('mouseenter', resetIdleIimer);
      document.removeEventListener('keydown', resetIdleIimer);
      document.removeEventListener('scroll', resetIdleIimer);
      document.removeEventListener('touchstart', resetIdleIimer);
      clearInterval(intervalId);
    }
  }, []);

  useEffect(() => {
    if (idleTimer <= 0) {
      window.location.assign(signout)
      // Idle timout => 自動登出
    }
  }, [idleTimer])

  return (
    <span className="mx-1">idle:{idleTimer}</span >
  )

  function decreaseIdleTimer() {
    setIdleTimer(timer => timer > 0 ? timer - 1 : 0)
  }

  function resetIdleIimer() {
    setIdleTimer(timeout || 100)
  }
}
