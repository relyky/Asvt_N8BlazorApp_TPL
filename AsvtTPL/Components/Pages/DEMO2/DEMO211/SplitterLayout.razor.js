// Grid Splitter JavaScript Interop
let gridSplitterInstance = null;

class GridSplitter {
    constructor(gridContainer, leftPanel, verticalDivider, horizontalDivider, minSize, initialLeftWidth, initialTopHeight) {
        this.gridContainer = gridContainer;
        this.leftPanel = leftPanel;
        this.verticalDivider = verticalDivider;
        this.horizontalDivider = horizontalDivider;
        this.minSize = minSize || 100;
        this.initialLeftWidth = initialLeftWidth || 50;
        this.initialTopHeight = initialTopHeight || 50;

        this.isDraggingVertical = false;
        this.isDraggingHorizontal = false;

        this.init();
    }

    init() {
        // 綁定垂直分隔線事件（左右分割）
        this.verticalMouseDownHandler = this.onVerticalMouseDown.bind(this);
        this.verticalDivider.addEventListener('mousedown', this.verticalMouseDownHandler);

        // 綁定水平分隔線事件（上下分割）
        this.horizontalMouseDownHandler = this.onHorizontalMouseDown.bind(this);
        this.horizontalDivider.addEventListener('mousedown', this.horizontalMouseDownHandler);

        // 綁定滑鼠移動和釋放事件
        this.mouseMoveHandler = this.onMouseMove.bind(this);
        this.mouseUpHandler = this.onMouseUp.bind(this);
        document.addEventListener('mousemove', this.mouseMoveHandler);
        document.addEventListener('mouseup', this.mouseUpHandler);

        // 初始化佈局
        this.resetLayout();
    }

    onVerticalMouseDown(e) {
        e.preventDefault();
        this.isDraggingVertical = true;
        this.verticalDivider.classList.add('dragging');
        document.body.style.cursor = 'col-resize';
        document.body.style.userSelect = 'none';
    }

    onHorizontalMouseDown(e) {
        e.preventDefault();
        this.isDraggingHorizontal = true;
        this.horizontalDivider.classList.add('dragging');
        document.body.style.cursor = 'row-resize';
        document.body.style.userSelect = 'none';
    }

    onMouseMove(e) {
        if (this.isDraggingVertical) {
            this.handleVerticalDrag(e);
        }
        if (this.isDraggingHorizontal) {
            this.handleHorizontalDrag(e);
        }
    }

    onMouseUp() {
        if (this.isDraggingVertical) {
            this.isDraggingVertical = false;
            this.verticalDivider.classList.remove('dragging');
            document.body.style.cursor = '';
            document.body.style.userSelect = '';
        }
        if (this.isDraggingHorizontal) {
            this.isDraggingHorizontal = false;
            this.horizontalDivider.classList.remove('dragging');
            document.body.style.cursor = '';
            document.body.style.userSelect = '';
        }
    }

    handleVerticalDrag(e) {
        const containerRect = this.gridContainer.getBoundingClientRect();
        const position = e.clientX - containerRect.left;
        const containerWidth = containerRect.width;

        // 計算左側寬度百分比
        let leftPercentage = (position / containerWidth) * 100;

        // 限制最小尺寸
        const minPercentage = (this.minSize / containerWidth) * 100;
        const dividerPercentage = (6 / containerWidth) * 100;
        const maxPercentage = 100 - minPercentage - dividerPercentage;

        leftPercentage = Math.max(minPercentage, Math.min(maxPercentage, leftPercentage));
        const rightPercentage = 100 - leftPercentage - dividerPercentage;

        // 更新 grid 佈局
        this.gridContainer.style.gridTemplateColumns = `${leftPercentage}% 6px ${rightPercentage}%`;
    }

    handleHorizontalDrag(e) {
        const panelRect = this.leftPanel.getBoundingClientRect();
        const position = e.clientY - panelRect.top;
        const panelHeight = panelRect.height;

        // 計算上方高度百分比
        let topPercentage = (position / panelHeight) * 100;

        // 限制最小尺寸
        const minPercentage = (this.minSize / panelHeight) * 100;
        const dividerPercentage = (6 / panelHeight) * 100;
        const maxPercentage = 100 - minPercentage - dividerPercentage;

        topPercentage = Math.max(minPercentage, Math.min(maxPercentage, topPercentage));
        const bottomPercentage = 100 - topPercentage - dividerPercentage;

        // 更新左側面板的 grid 佈局
        this.leftPanel.style.gridTemplateRows = `${topPercentage}% 6px ${bottomPercentage}%`;
    }

    resetLayout() {
        // 重置為初始分割比例
        const leftWidth = this.initialLeftWidth;
        const rightWidth = 100 - leftWidth - (6 / this.gridContainer.getBoundingClientRect().width * 100);
        this.gridContainer.style.gridTemplateColumns = `${leftWidth}% 6px ${rightWidth}%`;

        const topHeight = this.initialTopHeight;
        const bottomHeight = 100 - topHeight - (6 / this.leftPanel.getBoundingClientRect().height * 100);
        this.leftPanel.style.gridTemplateRows = `${topHeight}% 6px ${bottomHeight}%`;
    }

    dispose() {
        // 清理事件監聽器
        if (this.verticalDivider) {
            this.verticalDivider.removeEventListener('mousedown', this.verticalMouseDownHandler);
        }
        if (this.horizontalDivider) {
            this.horizontalDivider.removeEventListener('mousedown', this.horizontalMouseDownHandler);
        }
        document.removeEventListener('mousemove', this.mouseMoveHandler);
        document.removeEventListener('mouseup', this.mouseUpHandler);

        // 清理樣式
        document.body.style.cursor = '';
        document.body.style.userSelect = '';
    }
}

// 匯出函數供 Blazor 呼叫
export function initGridSplitter(gridContainer, leftPanel, verticalDivider, horizontalDivider, minSize, initialLeftWidth, initialTopHeight) {
    if (gridSplitterInstance) {
        gridSplitterInstance.dispose();
    }
    gridSplitterInstance = new GridSplitter(gridContainer, leftPanel, verticalDivider, horizontalDivider, minSize, initialLeftWidth, initialTopHeight);
}

export function resetGridLayout() {
    if (gridSplitterInstance) {
        gridSplitterInstance.resetLayout();
    }
}

export function disposeGridSplitter() {
    if (gridSplitterInstance) {
        gridSplitterInstance.dispose();
        gridSplitterInstance = null;
    }
}
