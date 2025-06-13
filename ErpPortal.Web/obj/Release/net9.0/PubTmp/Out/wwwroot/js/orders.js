// Global state
let selectedOrderId = null;
let userWorkCenters = [];

// Initial load
document.addEventListener('DOMContentLoaded', async () => {
    // await loadWorkCenters(); // Bu, loadOrders içinden çağrılacak
    // await loadOrders();
    setupWorkCenterFilter();
    checkHealthStatus();
    setInterval(checkHealthStatus, 30000);
});

// Load work centers from localStorage and set up the filter
function setupWorkCenterFilter() {
    try {
        const userJson = localStorage.getItem('user');
        if (!userJson) throw new Error("Kullanıcı bilgisi bulunamadı.");
        
        const user = JSON.parse(userJson);
        userWorkCenters = user.workCenters || [];

        const filterSelect = document.getElementById('workCenterFilter');
        filterSelect.innerHTML = ''; // Önce temizle

        if (userWorkCenters.length === 0) {
            filterSelect.innerHTML = '<option value="">Atanmış iş merkezi yok</option>';
            return;
        }

        userWorkCenters.forEach(wc => {
            const option = document.createElement('option');
            option.value = wc.code; // Değer olarak 'code' kullan
            option.textContent = `${wc.name} (${wc.code})`;
            filterSelect.appendChild(option);
        });

        // Filtre değiştiğinde operasyonları yeniden yükle
        filterSelect.onchange = () => loadOperations(filterSelect.value);

        // Sayfa yüklendiğinde ilk iş merkezi için operasyonları yükle
        if (filterSelect.value) {
            loadOperations(filterSelect.value);
        }

    } catch (error) {
        console.error('İş merkezi filtresi kurulurken hata:', error);
        document.getElementById('workCenterFilter').innerHTML = '<option value="">Filtre Yüklenemedi</option>';
    }
}

// Load orders for the selected work center
async function loadOrders() { /* Artık kullanılmıyor */ }

// Get status color class
function getStatusClass(status) {
    if (!status || typeof status !== 'string') return 'bg-gray-100 text-gray-800';
    status = status.toLowerCase();
    switch (status) {
        case 'released':
            return 'bg-blue-100 text-blue-800';
        case 'inprocess':
            return 'bg-yellow-100 text-yellow-800';
        case 'interruption':
            return 'bg-orange-100 text-orange-800';
        case 'partiallyreported':
            return 'bg-purple-100 text-purple-800';
        case 'closed':
            return 'bg-green-100 text-green-800';
        default:
            return 'bg-gray-100 text-gray-800';
    }
}

// Modal functions
function openNewOrderModal() {
    selectedOrderId = null;
    const modal = document.getElementById('orderModal');
    const title = document.getElementById('orderModalTitle');
    const form = document.getElementById('orderForm');

    title.textContent = 'New Order';
    form.reset();
    modal.classList.remove('hidden');
}

function closeOrderModal() {
    document.getElementById('orderModal').classList.add('hidden');
    selectedOrderId = null;
}

async function editOrder(orderId) {
    selectedOrderId = orderId;
    const modal = document.getElementById('orderModal');
    const title = document.getElementById('orderModalTitle');
    const form = document.getElementById('orderForm');

    title.textContent = 'Edit Order';
    form.reset();

    try {
        const response = await fetch(`/api/ShopOrder/${orderId}`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (!response.ok) throw new Error('Failed to load order');

        const order = await response.json();
        document.getElementById('workCenter').value = order.workCenterId;
        document.getElementById('description').value = order.description || '';
        modal.classList.remove('hidden');
    } catch (error) {
        console.error('Error loading order:', error);
        alert('Failed to load order details');
    }
}

async function handleOrderSubmit(event) {
    event.preventDefault();
    const formData = {
        workCenterId: document.getElementById('workCenter').value,
        description: document.getElementById('description').value
    };

    try {
        const url = selectedOrderId ? `/api/ShopOrder/${selectedOrderId}` : '/api/ShopOrder';
        const method = selectedOrderId ? 'PUT' : 'POST';
        
        if (selectedOrderId) {
            formData.id = selectedOrderId;
        }

        const response = await fetch(url, {
            method: method,
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });

        if (!response.ok) throw new Error('Failed to save order');

        closeOrderModal();
        loadOrders();
    } catch (error) {
        console.error('Error saving order:', error);
        alert('Failed to save order');
    }
}

async function deleteOrder(orderId) {
    if (!confirm('Are you sure you want to delete this order?')) return;

    try {
        const response = await fetch(`/api/ShopOrder/${orderId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (!response.ok) throw new Error('Failed to delete order');

        loadOrders();
    } catch (error) {
        console.error('Error deleting order:', error);
        alert('Failed to delete order');
    }
}

async function viewOrderDetails(orderId) {
    try {
        const response = await fetch(`/api/ShopOrder/${orderId}`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
        });

        if (!response.ok) throw new Error('Failed to load order details');

        const order = await response.json();
        const content = document.getElementById('orderDetailContent');
        content.innerHTML = `
            <div class="grid grid-cols-2 gap-4">
                <div>
                    <p class="text-sm font-medium text-gray-500">Order No</p>
                    <p class="mt-1">${order.orderNo}</p>
                </div>
                <div>
                    <p class="text-sm font-medium text-gray-500">Status</p>
                    <p class="mt-1">
                        <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusClass(order.status)}">
                            ${order.status}
                        </span>
                    </p>
                </div>
                <div>
                    <p class="text-sm font-medium text-gray-500">Work Center</p>
                    <p class="mt-1">${order.workCenter.name} (${order.workCenter.code})</p>
                </div>
                <div>
                    <p class="text-sm font-medium text-gray-500">Created Date</p>
                    <p class="mt-1">${new Date(order.createdDate).toLocaleString()}</p>
                </div>
                <div class="col-span-2">
                    <p class="text-sm font-medium text-gray-500">Description</p>
                    <p class="mt-1">${order.description || 'No description'}</p>
                </div>
            </div>
        `;

        document.getElementById('orderDetailModal').classList.remove('hidden');
    } catch (error) {
        console.error('Error loading order details:', error);
        alert('Failed to load order details');
    }
}

function closeOrderDetailModal() {
    document.getElementById('orderDetailModal').classList.add('hidden');
}

// Orders module
const orders = {
    currentOrderNo: '',
    currentOpNo: '',

    async loadOrders(workCenter) {
        try {
            const response = await fetch(`/api/shoporder/operations/${workCenter}`);
            if (response.ok) {
                return { success: true, data: await response.json() };
            }
            return { success: false, error: 'Failed to load orders' };
        } catch (error) {
            console.error('Error loading orders:', error);
            return { success: false, error: 'An error occurred while loading orders' };
        }
    },

    async getOrderDetail(orderNo, opNo) {
        this.currentOrderNo = orderNo;
        this.currentOpNo = opNo;
        
        try {
            const response = await fetch(`/api/shoporder/operation/${orderNo}/${opNo}`);
            if (response.ok) {
                return { success: true, data: await response.json() };
            }
            return { success: false, error: 'Failed to load order detail' };
        } catch (error) {
            console.error('Error loading order detail:', error);
            return { success: false, error: 'An error occurred while loading order detail' };
        }
    },

    async updateStatus(action) {
        try {
            const response = await fetch(
                `/api/shoporder/operation/${this.currentOrderNo}/${this.currentOpNo}/${action}`,
                { method: 'POST' }
            );
            
            if (response.ok) {
                return { success: true };
            }
            return { success: false, error: 'Failed to update operation status' };
        } catch (error) {
            console.error('Error updating operation status:', error);
            return { success: false, error: 'An error occurred while updating the operation' };
        }
    },

    getStatusColor(status) {
        const colors = {
            'Released': 'bg-blue-100 text-blue-800',
            'InProcess': 'bg-green-100 text-green-800',
            'Interruption': 'bg-yellow-100 text-yellow-800',
            'PartiallyReported': 'bg-purple-100 text-purple-800',
            'Closed': 'bg-gray-100 text-gray-800'
        };
        return colors[status] || colors.Closed;
    }
};

// Module exports removed for browser compatibility
async function updateStatus(orderNo, opNo, action, reason = '') {
    try {
        let url;
        if (action === 'start') {
            url = `/api/shoporder/${orderNo}/operations/${opNo}/start`;
        } else if (action === 'stop') {
            url = `/api/shoporder/${orderNo}/operations/${opNo}/stop`;
        } else if (action === 'complete') {
            url = `/api/shoporder/${orderNo}/operations/${opNo}/complete`;
        } else {
            console.error('Unknown action');
            return;
        }

        const options = {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        };

        if (action === 'stop' && reason) {
            options.body = JSON.stringify({ reason });
        }

        const response = await fetch(url, options);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        // Başarılı işlem sonrası sayfayı yenile
        window.location.reload();
    } catch (error) {
        console.error('Error:', error);
        alert('İşlem sırasında bir hata oluştu.');
    }
}

async function getOperationDetails(orderNo, opNo) {
    try {
        const response = await fetch(`/api/shoporder/${orderNo}/operations/${opNo}`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error('Error:', error);
        return null;
    }
}

function getAuthHeaders() {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/login.html';
        return null;
    }
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };
}

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('selectedWorkCenter');
    window.location.href = '/login.html';
}

document.addEventListener('DOMContentLoaded', () => {
    const user = JSON.parse(localStorage.getItem('user'));
    const workCenterCode = localStorage.getItem('selectedWorkCenter');

    if (!user || !workCenterCode) {
        logout();
        return;
    }

    document.getElementById('userInfo').textContent = `${user.fullName} (${workCenterCode})`;
    document.getElementById('pageTitle').textContent = `${workCenterCode} - İş Merkezi Operasyonları`;

    loadOperations(workCenterCode);
});

async function loadOperations(workCenterCode) {
    const headers = getAuthHeaders();
    if (!headers) return;

    const spinner = document.getElementById('loadingSpinner');
    const container = document.getElementById('operationsContainer');
    const tbody = document.getElementById('orderTableBody');

    spinner.classList.remove('hidden');
    container.classList.add('hidden');
    tbody.innerHTML = '';

    try {
        const response = await fetch(`/api/shoporder?workCenterCode=${workCenterCode}`, { headers });
        if (!response.ok) {
            if (response.status === 401) return logout();
            throw new Error('Operasyonlar yüklenemedi.');
        }

        const operations = await response.json();

        if (operations.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center py-4">Bu iş merkezine atanmış operasyon bulunmuyor.</td></tr>';
        } else {
            operations.forEach(op => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td class="px-6 py-4 whitespace-nowrap">${op.orderNo}</td>
                    <td class="px-6 py-4 whitespace-nowrap">${op.operationNo}</td>
                    <td class="px-6 py-4 whitespace-nowrap">${op.operationDescription}</td>
                    <td class="px-6 py-4 whitespace-nowrap">
                        <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${getStatusClass(op.operStatusCode)}">
                            ${op.operStatusCode}
                        </span>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                        <a href="/operation.html?orderNo=${op.orderNo}&opNo=${op.operationNo}" class="text-indigo-600 hover:text-indigo-900">Detay</a>
                    </td>
                `;
                tbody.appendChild(tr);
            });
        }
    } catch (error) {
        console.error('Hata:', error);
        tbody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-red-500">${error.message}</td></tr>`;
    } finally {
        spinner.classList.add('hidden');
        container.classList.remove('hidden');
    }
}

function closeOperationModal() {
    document.getElementById('operationModal').classList.add('hidden');
    document.getElementById('operationModalContent').innerHTML = '';
}

async function showOperationDetails(orderNo, opNo) {
    const headers = getAuthHeaders();
    if (!headers) return;
    
    const modal = document.getElementById('operationModal');
    const modalContent = document.getElementById('operationModalContent');
    modalContent.innerHTML = '<p>Yükleniyor...</p>';
    modal.classList.remove('hidden');

    try {
        const safeOrder = encodeURIComponent(orderNo);
        const response = await fetch(`/api/shoporder/operation/${safeOrder}/${opNo}`, { headers });
        if (!response.ok) throw new Error('Operasyon detayı alınamadı.');
        
        const op = await response.json();
        
        // Modal içeriğini operasyon detayları ve durumuna uygun eylem butonları ile doldur
        let content = `
            <div><strong>İş Emri:</strong> ${op.orderNo}</div>
            <div><strong>Operasyon:</strong> ${op.operationNo}</div>
            <div><strong>Parça No:</strong> ${op.partNo}</div>
            <div><strong>Açıklama:</strong> ${op.operationDescription}</div>
            <div><strong>Durum:</strong> ${op.operStatusCode}</div>
            <hr class="my-4">
            <div class="flex justify-end space-x-4">`;

        // Duruma göre butonları ekle
        if (op.operStatusCode.toLowerCase() !== 'started') {
            content += `<button onclick="performAction('${op.orderNo}', ${op.operationNo}, 'start')" class="bg-green-500 text-white px-4 py-2 rounded">Başlat</button>`;
        }
        if (op.operStatusCode.toLowerCase() === 'started') {
             content += `<button onclick="showStopForm('${op.orderNo}', ${op.operationNo})" class="bg-yellow-500 text-white px-4 py-2 rounded">Durdur</button>`;
             content += `<button onclick="showCompleteForm('${op.orderNo}', ${op.operationNo})" class="bg-blue-500 text-white px-4 py-2 rounded">Tamamla</button>`;
        }
        
        content += `<button onclick="closeOperationModal()" class="bg-gray-300 text-gray-800 px-4 py-2 rounded">Kapat</button></div>`;
        modalContent.innerHTML = content;

    } catch (error) {
        modalContent.innerHTML = `<p class="text-red-500">${error.message}</p>`;
    }
}

async function performAction(orderNo, opNo, action, data = {}) {
    const headers = getAuthHeaders();
    if (!headers) return;

    const safeOrder2 = encodeURIComponent(orderNo);
    let url = `/api/shoporder/operation/${safeOrder2}/${opNo}/${action}`;
    
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: headers,
            body: Object.keys(data).length ? JSON.stringify(data) : null
        });

        if (!response.ok) {
            const errData = await response.text();
            throw new Error(`İşlem başarısız: ${errData}`);
        }

        closeOperationModal();
        loadOperations(localStorage.getItem('selectedWorkCenter')); // Listeyi yenile

    } catch (error) {
        alert(error.message);
    }
}

// TODO: Implement showStopForm and showCompleteForm to render forms inside the modal
// These functions would replace the modal content with a form and a submit button
// that calls performAction with the form data.

// Example for showStopForm
function showStopForm(orderNo, opNo) {
    const modalContent = document.getElementById('operationModalContent');
    modalContent.innerHTML = `
        <h4 class="font-bold mb-2">Operasyonu Durdur</h4>
        <label for="stopReason" class="block text-sm font-medium text-gray-700">Durdurma Nedeni</label>
        <textarea id="stopReason" rows="3" class="w-full border rounded p-2 mt-1"></textarea>
        <div class="flex justify-end space-x-4 mt-4">
            <button onclick="performAction('${orderNo}', ${opNo}, 'stop', { reason: document.getElementById('stopReason').value })" class="bg-yellow-500 text-white px-4 py-2 rounded">Onayla</button>
            <button onclick="showOperationDetails('${orderNo}', ${opNo})" class="bg-gray-300 text-gray-800 px-4 py-2 rounded">İptal</button>
        </div>
    `;
}

// Example for showCompleteForm
function showCompleteForm(orderNo, opNo) {
     const modalContent = document.getElementById('operationModalContent');
    modalContent.innerHTML = `
        <h4 class="font-bold mb-2">Operasyonu Tamamla</h4>
        <div class="grid grid-cols-2 gap-4">
            <div>
                <label for="qtyCompleted" class="block text-sm font-medium text-gray-700">Tamamlanan Miktar</label>
                <input type="number" id="qtyCompleted" value="0" class="w-full border rounded p-2 mt-1">
            </div>
            <div>
                <label for="qtyScrapped" class="block text-sm font-medium text-gray-700">Hurda Miktar</label>
                <input type="number" id="qtyScrapped" value="0" class="w-full border rounded p-2 mt-1">
            </div>
        </div>
        <div class="flex justify-end space-x-4 mt-4">
            <button onclick="performAction('${orderNo}', ${opNo}, 'complete', { quantityCompleted: document.getElementById('qtyCompleted').value, quantityScrapped: document.getElementById('qtyScrapped').value })" class="bg-blue-500 text-white px-4 py-2 rounded">Raporla</button>
            <button onclick="showOperationDetails('${orderNo}', ${opNo})" class="bg-gray-300 text-gray-800 px-4 py-2 rounded">İptal</button>
        </div>
    `;
}

// ----------------------- Sistem Sağlık Kontrolü ---------------------------
async function checkHealthStatus() {
    const dot = document.getElementById('healthDot');
    const text = document.getElementById('healthText');

    if (!dot || !text) return; // HTML bulunamazsa çık

    try {
        const resp = await fetch('/api/system/health', {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('token') ?? ''}`
            }
        });

        if (!resp.ok) throw new Error('Sunucu yanıtı alınamadı');

        const data = await resp.json();
        // Beklenen formatlardan bazıları:
        // 1) { ifsApi: true }
        // 2) { IfsApi: true }
        // 3) { apiStatus: 'OK' }
        // Herhangi birinde olumlu durum varsa apiOk=true
        let apiOk = false;
        if (typeof data === 'boolean') {
            apiOk = data === true;
        } else if (data) {
            if (typeof data.IfsApi === 'boolean') apiOk = data.IfsApi;
            else if (typeof data.ifsApi === 'boolean') apiOk = data.ifsApi;
            else if (typeof data.apiStatus === 'string') apiOk = data.apiStatus.toUpperCase() === 'OK';
        }

        if (apiOk) {
            dot.className = 'inline-block w-3 h-3 rounded-full bg-green-500 mr-1';
            text.textContent = 'Online';
        } else {
            dot.className = 'inline-block w-3 h-3 rounded-full bg-red-500 mr-1';
            text.textContent = 'Offline';
        }
    } catch (err) {
        dot.className = 'inline-block w-3 h-3 rounded-full bg-red-500 mr-1';
        text.textContent = 'Offline';
    }
} 