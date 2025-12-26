// DecisionTreeEditor JavaScript Interop
// 處理拖拉功能、檔案下載、通知等瀏覽器專屬功能

let dotNetReference = null;
let draggedElement = null;
let draggedNodeId = null;
let draggedParentId = null;

/**
 * 初始化拖拉功能
 * @param {any} dotNetRef - DotNetObjectReference，用於呼叫 C# 方法
 */
export function initializeDragDrop(dotNetRef) {
    dotNetReference = dotNetRef;

    // 使用事件委派，監聽整個 tree-content 區域
    document.addEventListener('dragstart', handleDragStart, true);
    document.addEventListener('dragover', handleDragOver, true);
    document.addEventListener('drop', handleDrop, true);
    document.addEventListener('dragenter', handleDragEnter, true);
    document.addEventListener('dragleave', handleDragLeave, true);
    document.addEventListener('dragend', handleDragEnd, true);

    console.log('DecisionTreeEditor: Drag & Drop initialized');
}

/**
 * 開始拖拉
 */
function handleDragStart(e) {
    const nodeContent = e.target.closest('.dt-node-content');
    if (!nodeContent) return;

    draggedElement = nodeContent;
    draggedNodeId = nodeContent.getAttribute('data-node-id');
    draggedParentId = nodeContent.getAttribute('data-parent-id');

    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('text/html', nodeContent.innerHTML);
    nodeContent.style.opacity = '0.5';

    // 通知 C# 開始拖拉
    if (dotNetReference) {
        dotNetReference.invokeMethodAsync('OnDragStart', draggedNodeId, draggedParentId);
    }

    showDragHint(e, '拖曳中: ' + nodeContent.querySelector('.dt-node-label')?.textContent);
}

/**
 * 拖拉經過
 */
function handleDragOver(e) {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';

    const nodeContent = e.target.closest('.dt-node-content');
    if (!nodeContent || nodeContent === draggedElement) return;

    // 位置偵測邏輯
    const rect = nodeContent.getBoundingClientRect();
    const mouseY = e.clientY;
    const nodeMiddle = rect.top + (rect.height / 2);

    // 上半部 = above（兄弟），下半部 = below（子節點）
    const dropPosition = mouseY < nodeMiddle ? 'above' : 'below';
    nodeContent.setAttribute('data-drop-position', dropPosition);

    updateDropIndicator(nodeContent, dropPosition);
    updateDragHintPosition(e);

    return false;
}

/**
 * 拖拉進入
 */
function handleDragEnter(e) {
    const nodeContent = e.target.closest('.dt-node-content');
    if (nodeContent && nodeContent !== draggedElement) {
        nodeContent.classList.add('drag-over');
    }
}

/**
 * 拖拉離開
 */
function handleDragLeave(e) {
    const nodeContent = e.target.closest('.dt-node-content');
    if (nodeContent) {
        nodeContent.classList.remove('drag-over', 'drop-above', 'drop-below');
        nodeContent.removeAttribute('data-drop-position');
    }
}

/**
 * 放下
 */
async function handleDrop(e) {
    e.stopPropagation();
    e.preventDefault();

    const nodeContent = e.target.closest('.dt-node-content');
    if (!nodeContent) return false;

    nodeContent.classList.remove('drag-over', 'drop-above', 'drop-below');

    const targetNodeId = nodeContent.getAttribute('data-node-id');
    const dropPosition = nodeContent.getAttribute('data-drop-position') || 'below';

    // 呼叫 C# 方法處理節點移動
    if (dotNetReference && draggedNodeId && targetNodeId) {
        try {
            const result = await dotNetReference.invokeMethodAsync('OnDrop', targetNodeId, dropPosition);
            if (!result) {
                console.warn('Drop operation was rejected by the server');
            }
        } catch (error) {
            console.error('Error during drop:', error);
        }
    }

    return false;
}

/**
 * 拖拉結束
 */
function handleDragEnd(e) {
    if (draggedElement) {
        draggedElement.style.opacity = '1';
    }

    hideDragHint();

    // 清理所有拖拉相關樣式和屬性
    document.querySelectorAll('.drag-over, .drop-above, .drop-below').forEach(el => {
        el.classList.remove('drag-over', 'drop-above', 'drop-below');
        el.removeAttribute('data-drop-position');
    });

    draggedElement = null;
    draggedNodeId = null;
    draggedParentId = null;
}

/**
 * 更新拖拉位置指示器
 */
function updateDropIndicator(nodeContent, position) {
    // 移除舊樣式
    nodeContent.classList.remove('drop-above', 'drop-below');

    // 添加新樣式
    if (position === 'above') {
        nodeContent.classList.add('drop-above');
    } else {
        nodeContent.classList.add('drop-below');
    }
}

/**
 * 顯示拖拉提示
 */
function showDragHint(e, text) {
    let hint = document.getElementById('dragHint');
    if (!hint) {
        hint = document.createElement('div');
        hint.id = 'dragHint';
        hint.className = 'dt-drag-hint';
        document.body.appendChild(hint);
    }
    hint.textContent = text;
    hint.style.display = 'block';
    hint.style.left = (e.pageX + 10) + 'px';
    hint.style.top = (e.pageY + 10) + 'px';
}

/**
 * 更新拖拉提示位置
 */
function updateDragHintPosition(e) {
    const hint = document.getElementById('dragHint');
    if (hint) {
        hint.style.left = (e.pageX + 10) + 'px';
        hint.style.top = (e.pageY + 10) + 'px';
    }
}

/**
 * 隱藏拖拉提示
 */
function hideDragHint() {
    const hint = document.getElementById('dragHint');
    if (hint) {
        hint.style.display = 'none';
    }
}

/**
 * 觸發檔案輸入對話框
 * @param {string} elementId - 檔案輸入元素的 ID
 */
export function triggerFileInput(elementId) {
    const fileInput = document.getElementById(elementId);
    if (fileInput) {
        fileInput.click();
    } else {
        console.error(`File input element with id '${elementId}' not found`);
    }
}

/**
 * 下載檔案
 * @param {string} filename - 檔案名稱
 * @param {string} content - 檔案內容
 * @param {string} mimeType - MIME 類型
 */
export function downloadFile(filename, content, mimeType) {
    const blob = new Blob([content], { type: mimeType + ';charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

/**
 * 顯示通知
 * @param {string} message - 訊息內容
 * @param {string} type - 訊息類型 (success, error, warning, info)
 */
export function showNotification(message, type = 'info') {
    // 建立通知元素
    const notification = document.createElement('div');
    notification.className = `dt-validation-${type}`;
    notification.textContent = message;
    notification.style.position = 'fixed';
    notification.style.top = '20px';
    notification.style.right = '20px';
    notification.style.zIndex = '9999';
    notification.style.minWidth = '200px';
    notification.style.boxShadow = '0 4px 6px rgba(0,0,0,0.1)';

    document.body.appendChild(notification);

    // 3 秒後自動移除
    setTimeout(() => {
        notification.style.transition = 'opacity 0.5s';
        notification.style.opacity = '0';
        setTimeout(() => {
            if (notification.parentNode) {
                document.body.removeChild(notification);
            }
        }, 500);
    }, 3000);
}

/**
 * 複製到剪貼簿
 * @param {string} text - 要複製的文字
 */
export async function copyToClipboard(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (error) {
        console.error('Failed to copy to clipboard:', error);
        // 降級方案：使用舊方法
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.left = '-999999px';
        document.body.appendChild(textArea);
        textArea.select();
        try {
            document.execCommand('copy');
            document.body.removeChild(textArea);
            return true;
        } catch (err) {
            document.body.removeChild(textArea);
            console.error('Fallback copy failed:', err);
            return false;
        }
    }
}

/**
 * 清理資源（當元件被銷毀時呼叫）
 */
export function dispose() {
    document.removeEventListener('dragstart', handleDragStart, true);
    document.removeEventListener('dragover', handleDragOver, true);
    document.removeEventListener('drop', handleDrop, true);
    document.removeEventListener('dragenter', handleDragEnter, true);
    document.removeEventListener('dragleave', handleDragLeave, true);
    document.removeEventListener('dragend', handleDragEnd, true);

    const hint = document.getElementById('dragHint');
    if (hint && hint.parentNode) {
        hint.parentNode.removeChild(hint);
    }

    console.log('DecisionTreeEditor: Resources disposed');
}
