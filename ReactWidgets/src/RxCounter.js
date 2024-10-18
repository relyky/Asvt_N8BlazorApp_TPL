import React, { useState } from 'react'

const btnPrimary = {
  color: '#fff',
  backgroundColor: '#0d6efd',
  padding: 4,
  borderRadius: 4
}

export default function RxCounter({ initCount /* int */, onChange /* event */ }) {
  const [count, setCount] = useState(initCount || 0);

  function handleClick() {
    const newCount = count + 1;
    setCount(newCount)
    onChange(newCount)
  }

  return (
    <div className="pa-2 my-2" style={{ border: 'solid 2px red', borderRadius: 8 }}>
      <h3>我用 React 開發出來的</h3>
      <p>You clicked {count} times</p>
      <button style={btnPrimary} onClick={handleClick}>
        Click me
      </button>
    </div>
  )
}
