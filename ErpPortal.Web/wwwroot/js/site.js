// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

import { updateStatus, getOperationDetails } from './orders.js';
import { showLoading, hideLoading, updateUI } from './ui.js';
import { checkAuth } from './auth.js';

let currentWorkCenter = '';
let currentOrderNo = '';
let currentOpNo = '';

// Event listeners
document.addEventListener('DOMContentLoaded', async () => {
    await checkAuth();

    const loginForm = document.getElementById('loginFormSubmit');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }

    // Operation control buttons
    document.querySelectorAll('.btn-start-operation').forEach(button => {
        button.addEventListener('click', () => handleOperationAction(button, 'start'));
    });

    document.querySelectorAll('.btn-stop-operation').forEach(button => {
        button.addEventListener('click', () => handleOperationAction(button, 'stop'));
    });

    document.querySelectorAll('.btn-resume-operation').forEach(button => {
        button.addEventListener('click', () => handleOperationAction(button, 'resume'));
    });

    document.querySelectorAll('.btn-complete-operation').forEach(button => {
        button.addEventListener('click', () => handleOperationAction(button, 'complete'));
    });

    document.querySelectorAll('.btn-scrap-report').forEach(button => {
        button.addEventListener('click', () => handleScrapReport(button));
    });
});

// Login handler
async function handleLogin(e) {
    e.preventDefault();
    const username = document.getElementById('username').value;
    const workCenterCode = document.getElementById('password').value;
    
    const result = await auth.login(username, workCenterCode);
    if (result.success) {
        ui.showWorkCenter(result.data.workCenter);
        ui.switchToOrderList();
        await loadOrders();
    } else {
        ui.showError(result.error);
    }
}

// Order operations
async function loadOrders() {
    const result = await orders.loadOrders(auth.currentWorkCenter);
    if (result.success) {
        ui.updateOrderList(result.data, orders.getStatusColor);
    } else {
        ui.showError(result.error);
    }
}

// Make these functions available globally for button click handlers
window.viewOrderDetail = async function(orderNo, opNo) {
    const result = await orders.getOrderDetail(orderNo, opNo);
    if (result.success) {
        ui.updateOrderDetail(result.data);
        ui.switchToOrderDetail();
    } else {
        ui.showError(result.error);
    }
};

window.backToOrders = function() {
    ui.switchToOrderList();
    loadOrders();
};

async function handleOperationAction(button, action) {
    const orderNo = button.getAttribute('data-order-no');
    const opNo = button.getAttribute('data-op-no');

    if (!orderNo || !opNo) {
        console.error('Missing order or operation number');
        return;
    }

    showLoading();

    try {
        let reason = '';
        if (action === 'stop') {
            reason = prompt('Lütfen durma sebebini girin:');
            if (reason === null) { // Kullanıcı iptal ettiyse
                hideLoading();
                return;
            }
            if (!reason.trim()) {
                alert('Durma sebebi boş olamaz!');
                hideLoading();
                return;
            }
        }

        await updateStatus(orderNo, opNo, action, reason);
        
        // UI'ı güncelle
        const operation = await getOperationDetails(orderNo, opNo);
        if (operation) {
            updateUI(operation);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('İşlem sırasında bir hata oluştu.');
    } finally {
        hideLoading();
    }
}

async function handleScrapReport(button) {
    const orderNo = button.getAttribute('data-order-no');
    const opNo = button.getAttribute('data-op-no');

    if (!orderNo || !opNo) {
        console.error('Missing order or operation number');
        return;
    }

    const qty = prompt('Hurda miktarını girin:');
    if (qty === null) return; // Kullanıcı iptal ettiyse

    const qtyNumber = parseInt(qty);
    if (isNaN(qtyNumber) || qtyNumber < 0) {
        alert('Geçerli bir miktar giriniz!');
        return;
    }

    const reason = prompt('Hurda sebebini girin:');
    if (reason === null) return; // Kullanıcı iptal ettiyse
    if (!reason.trim()) {
        alert('Hurda sebebi boş olamaz!');
        return;
    }

    showLoading();

    try {
        await updateStatus(orderNo, opNo, 'scrap', { qty: qtyNumber, reason });
        
        // UI'ı güncelle
        const operation = await getOperationDetails(orderNo, opNo);
        if (operation) {
            updateUI(operation);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('İşlem sırasında bir hata oluştu.');
    } finally {
        hideLoading();
    }
}
