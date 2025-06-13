// UI module
const ui = {
    elements: {
        loginForm: () => document.getElementById('loginForm'),
        orderList: () => document.getElementById('orderList'),
        orderDetail: () => document.getElementById('orderDetail'),
        errorMessage: () => document.getElementById('errorMessage'),
        workCenterInfo: () => document.getElementById('workCenterInfo'),
        orderTableBody: () => document.getElementById('orderTableBody'),
        detailOrderNo: () => document.getElementById('detailOrderNo'),
        detailOperation: () => document.getElementById('detailOperation'),
        detailStatus: () => document.getElementById('detailStatus'),
        detailQtyRequired: () => document.getElementById('detailQtyRequired'),
        detailQtyCompleted: () => document.getElementById('detailQtyCompleted'),
        detailQtyScrapped: () => document.getElementById('detailQtyScrapped')
    },

    showError(message) {
        const errorEl = this.elements.errorMessage();
        if (errorEl) {
            errorEl.textContent = message;
            errorEl.classList.remove('hidden');
        }
    },

    hideError() {
        const errorEl = this.elements.errorMessage();
        if (errorEl) {
            errorEl.classList.add('hidden');
        }
    },

    showWorkCenter(workCenter) {
        const wcInfo = this.elements.workCenterInfo();
        if (wcInfo) {
            wcInfo.textContent = `Work Center: ${workCenter}`;
        }
    },

    switchToOrderList() {
        this.elements.loginForm()?.classList.add('hidden');
        this.elements.orderDetail()?.classList.add('hidden');
        this.elements.orderList()?.classList.remove('hidden');
    },

    switchToOrderDetail() {
        this.elements.orderList()?.classList.add('hidden');
        this.elements.orderDetail()?.classList.remove('hidden');
    },

    updateOrderList(orders, statusColorFn) {
        const tableBody = this.elements.orderTableBody();
        if (!tableBody) return;

        tableBody.innerHTML = '';
        orders.forEach(order => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">${order.orderNo}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${order.operationNo} - ${order.operationDescription}</td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${statusColorFn(order.operStatusCode)}">
                        ${order.operStatusCode}
                    </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    ${order.qtyComplete} / ${order.revisedQtyDue}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <button onclick="window.viewOrderDetail('${order.orderNo}', ${order.operationNo})" 
                        class="text-indigo-600 hover:text-indigo-900">
                        View Details
                    </button>
                </td>
            `;
            tableBody.appendChild(row);
        });
    },

    updateOrderDetail(detail) {
        this.elements.detailOrderNo().textContent = `Order: ${detail.orderNo}`;
        this.elements.detailOperation().textContent = `Operation ${detail.operationNo}: ${detail.operationDescription}`;
        this.elements.detailStatus().textContent = detail.operStatusCode;
        this.elements.detailQtyRequired().textContent = detail.revisedQtyDue;
        this.elements.detailQtyCompleted().textContent = detail.qtyComplete;
        this.elements.detailQtyScrapped().textContent = detail.qtyScrapped;

        // Set data attributes for all operation buttons
        document.querySelectorAll('[data-order-no]').forEach(button => {
            button.setAttribute('data-order-no', detail.orderNo);
            button.setAttribute('data-op-no', detail.operationNo);
        });

        // Update button states based on operation status
        updateButtons(detail);
    }
};

export default ui;

export function showLoading() {
    const loadingElement = document.getElementById('loading');
    if (loadingElement) {
        loadingElement.style.display = 'block';
    }
}

export function hideLoading() {
    const loadingElement = document.getElementById('loading');
    if (loadingElement) {
        loadingElement.style.display = 'none';
    }
}

export function updateUI(operation) {
    // Status badge güncelleme
    const statusBadge = document.querySelector(`[data-order-status="${operation.orderNo}-${operation.operationNo}"]`);
    if (statusBadge) {
        statusBadge.textContent = operation.operStatusCode;
        statusBadge.className = `badge ${getStatusClass(operation.operStatusCode)}`;
    }

    // Butonların durumunu güncelle
    updateButtons(operation);
}

function updateButtons(operation) {
    const startBtn = document.querySelector(`.btn-start-operation[data-order-no="${operation.orderNo}"][data-op-no="${operation.operationNo}"]`);
    const stopBtn = document.querySelector(`.btn-stop-operation[data-order-no="${operation.orderNo}"][data-op-no="${operation.operationNo}"]`);
    const completeBtn = document.querySelector(`.btn-complete-operation[data-order-no="${operation.orderNo}"][data-op-no="${operation.operationNo}"]`);

    if (startBtn) startBtn.disabled = operation.operStatusCode !== 'Released';
    if (stopBtn) stopBtn.disabled = operation.operStatusCode !== 'InProcess';
    if (completeBtn) completeBtn.disabled = operation.operStatusCode !== 'InProcess' && operation.operStatusCode !== 'Interruption';
}

function getStatusClass(status) {
    switch (status) {
        case 'Released':
            return 'bg-primary';
        case 'InProcess':
            return 'bg-success';
        case 'Interruption':
            return 'bg-warning';
        case 'Closed':
            return 'bg-secondary';
        default:
            return 'bg-info';
    }
} 