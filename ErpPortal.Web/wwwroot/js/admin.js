function getAuthHeaders() {
    const token = localStorage.getItem('token');
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };
}

// Authentication and authorization check
async function checkAdminAuth() {
    const token = localStorage.getItem('token');
    if (!token) {
        window.location.href = '/login.html';
        return false;
    }

    try {
        const response = await fetch('/api/account/users/me', {
            method: 'GET',
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            // Token is invalid or expired. Clear it and redirect to login.
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            window.location.href = '/login.html';
            return false;
        }

        const user = await response.json();
        
        if (!user.isAdmin) {
            // Not admin - show access denied message and redirect
            alert('Access Denied: Admin privileges required');
            window.location.href = '/orders.html'; // Redirect to normal user page
            return false;
        }

        // Authorized - show user info and content
        document.getElementById('userInfo').textContent = `${user.fullName} (Admin)`;
        document.getElementById('loadingSpinner').classList.add('hidden');
        document.getElementById('mainContent').classList.remove('hidden');
        
        return true;
    } catch (error) {
        console.error('Auth check error:', error);
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/login.html';
        return false;
    }
}

// Logout function
function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/login.html';
}

// Initialize page
document.addEventListener('DOMContentLoaded', async () => {
    const isAuthorized = await checkAdminAuth();
    if (isAuthorized) {
        // Load initial data
        loadUsers();
        loadWorkCenters();
    }
});

// Global state
let selectedUserId = null;
let selectedWorkCenterId = null;

// Tab switching
function switchTab(tabName) {
    // Update tab buttons
    document.querySelectorAll('#tabButtons button').forEach(button => {
        if (button.dataset.tab === tabName) {
            button.classList.add('text-blue-600', 'border-blue-600');
            button.classList.remove('text-gray-500', 'hover:text-gray-600', 'hover:border-gray-300');
        } else {
            button.classList.remove('text-blue-600', 'border-blue-600');
            button.classList.add('text-gray-500', 'hover:text-gray-600', 'hover:border-gray-300');
        }
    });

    // Show selected tab content
    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.add('hidden');
    });
    document.getElementById(`${tabName}Tab`).classList.remove('hidden');
}

// Users
async function loadUsers() {
    try {
        const response = await fetch('/api/users', {
            headers: getAuthHeaders()
        });
        const users = await response.json();
        
        const tbody = document.getElementById('userTableBody');
        tbody.innerHTML = users.map(user => `
            <tr>
                <td class="px-6 py-4 whitespace-nowrap">${user.username}</td>
                <td class="px-6 py-4 whitespace-nowrap">${user.fullName}</td>
                <td class="px-6 py-4 whitespace-nowrap">${user.isAdmin ? 'Yes' : 'No'}</td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <button onclick="openWorkCenterAssignmentModal(${user.id})" 
                            class="text-blue-600 hover:text-blue-800">
                        Manage Work Centers
                    </button>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <button onclick="openUserModal(${user.id})" 
                            class="text-blue-600 hover:text-blue-800 mr-2">
                        Edit
                    </button>
                    <button onclick="deleteUser(${user.id})" 
                            class="text-red-600 hover:text-red-800">
                        Delete
                    </button>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        console.error('Error loading users:', error);
        alert('Failed to load users');
    }
}

function openUserModal(userId = null) {
    const modal = document.getElementById('userModal');
    const title = document.getElementById('userModalTitle');
    const form = document.getElementById('userForm');
    
    title.textContent = userId ? 'Edit User' : 'Add User';
    form.reset();
    
    if (userId) {
        // Load user data for editing
        fetch(`/api/users/${userId}`, {
            headers: getAuthHeaders()
        })
        .then(response => response.json())
        .then(user => {
            document.getElementById('username').value = user.username;
            document.getElementById('fullName').value = user.fullName;
            document.getElementById('isAdmin').checked = user.isAdmin;
            form.dataset.userId = userId;
        });
    } else {
        delete form.dataset.userId;
    }
    
    modal.classList.remove('hidden');
}

function closeUserModal() {
    document.getElementById('userModal').classList.add('hidden');
}

async function handleUserSubmit(event) {
    event.preventDefault();
    
    const form = event.target;
    const userId = form.dataset.userId;
    const userData = {
        username: form.username.value,
        fullName: form.fullName.value,
        password: form.password.value,
        isAdmin: form.isAdmin.checked
    };
    
    try {
        const response = await fetch(userId ? `/api/users/${userId}` : '/api/users', {
            method: userId ? 'PUT' : 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify(userData)
        });
        
        if (!response.ok) {
            throw new Error('Failed to save user');
        }
        
        closeUserModal();
        await loadUsers();
    } catch (error) {
        console.error('Error saving user:', error);
        alert('Failed to save user');
    }
}

async function deleteUser(userId) {
    if (!confirm('Are you sure you want to delete this user?')) {
        return;
    }
    
    try {
        const response = await fetch(`/api/users/${userId}`, {
            method: 'DELETE',
            headers: getAuthHeaders()
        });
        
        if (!response.ok) {
            throw new Error('Failed to delete user');
        }
        
        await loadUsers();
    } catch (error) {
        console.error('Error deleting user:', error);
        alert('Failed to delete user');
    }
}

// Work Centers
async function loadWorkCenters() {
    try {
        const response = await fetch('/api/workcenters', {
            headers: getAuthHeaders()
        });
        const workCenters = await response.json();
        
        const tbody = document.getElementById('workCenterTableBody');
        tbody.innerHTML = workCenters.map(wc => `
            <tr>
                <td class="px-6 py-4 whitespace-nowrap">${wc.code}</td>
                <td class="px-6 py-4 whitespace-nowrap">${wc.name}</td>
                <td class="px-6 py-4 whitespace-nowrap">${wc.description || ''}</td>
                <td class="px-6 py-4 whitespace-nowrap">${wc.isActive ? 'Active' : 'Inactive'}</td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <button onclick="openWorkCenterModal(${wc.id})" 
                            class="text-blue-600 hover:text-blue-800 mr-2">
                        Edit
                    </button>
                    <button onclick="deleteWorkCenter(${wc.id})" 
                            class="text-red-600 hover:text-red-800">
                        Delete
                    </button>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        console.error('Error loading work centers:', error);
        alert('Failed to load work centers');
    }
}

function openWorkCenterModal(workCenterId = null) {
    const modal = document.getElementById('workCenterModal');
    const title = document.getElementById('workCenterModalTitle');
    const form = document.getElementById('workCenterForm');
    
    title.textContent = workCenterId ? 'Edit Work Center' : 'Add Work Center';
    form.reset();
    
    if (workCenterId) {
        // Load work center data for editing
        fetch(`/api/workcenters/${workCenterId}`, {
            headers: getAuthHeaders()
        })
        .then(response => response.json())
        .then(workCenter => {
            document.getElementById('code').value = workCenter.code;
            document.getElementById('name').value = workCenter.name;
            document.getElementById('description').value = workCenter.description || '';
            document.getElementById('isActive').checked = workCenter.isActive;
            form.dataset.workCenterId = workCenterId;
        });
    } else {
        delete form.dataset.workCenterId;
    }
    
    modal.classList.remove('hidden');
}

function closeWorkCenterModal() {
    document.getElementById('workCenterModal').classList.add('hidden');
}

async function handleWorkCenterSubmit(event) {
    event.preventDefault();
    
    const form = event.target;
    const workCenterId = form.dataset.workCenterId;
    const workCenterData = {
        code: form.code.value,
        name: form.name.value,
        description: form.description.value,
        isActive: form.isActive.checked
    };
    
    try {
        const response = await fetch(workCenterId ? `/api/workcenters/${workCenterId}` : '/api/workcenters', {
            method: workCenterId ? 'PUT' : 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify(workCenterData)
        });
        
        if (!response.ok) {
            throw new Error('Failed to save work center');
        }
        
        closeWorkCenterModal();
        await loadWorkCenters();
    } catch (error) {
        console.error('Error saving work center:', error);
        alert('Failed to save work center');
    }
}

async function deleteWorkCenter(workCenterId) {
    if (!confirm('Are you sure you want to delete this work center?')) {
        return;
    }
    
    try {
        const response = await fetch(`/api/workcenters/${workCenterId}`, {
            method: 'DELETE',
            headers: getAuthHeaders()
        });
        
        if (!response.ok) {
            throw new Error('Failed to delete work center');
        }
        
        await loadWorkCenters();
    } catch (error) {
        console.error('Error deleting work center:', error);
        alert('Failed to delete work center');
    }
}

// Work Center Assignment
async function openWorkCenterAssignmentModal(userId) {
    selectedUserId = userId;
    const modal = document.getElementById('workCenterAssignmentModal');
    const list = document.getElementById('workCenterList');
    const title = document.getElementById('workCenterAssignmentModalTitle');
    list.innerHTML = '<li>Loading...</li>';
    modal.classList.remove('hidden');

    try {
        // Fetch all available work centers and the user's current assignments in parallel
        const [workCentersResponse, userResponse] = await Promise.all([
            fetch('/api/workcenters', { headers: getAuthHeaders() }),
            fetch(`/api/account/users/${userId}`, { headers: getAuthHeaders() })
        ]);

        if (!workCentersResponse.ok) throw new Error('Failed to load work centers.');
        if (!userResponse.ok) throw new Error('Failed to load user data.');

        const allWorkCenters = await workCentersResponse.json();
        const user = await userResponse.json();
        
        const assignedWorkCenterIds = new Set(user.workCenters.map(wc => wc.id));
        title.textContent = `Manage Work Centers for ${user.username}`;

        list.innerHTML = allWorkCenters.map(wc => `
            <li class="flex items-center justify-between py-2">
                <span>${wc.name} (${wc.code})</span>
                <label class="inline-flex items-center cursor-pointer">
                    <input type="checkbox" 
                           class="sr-only peer" 
                           onchange="toggleWorkCenterAssignment(${userId}, ${wc.id}, this.checked)"
                           ${assignedWorkCenterIds.has(wc.id) ? 'checked' : ''}>
                    <div class="relative w-11 h-6 bg-gray-200 rounded-full peer peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 dark:bg-gray-700 peer-checked:after:translate-x-full rtl:peer-checked:after:-translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-0.5 after:start-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-600"></div>
                </label>
            </li>
        `).join('');

    } catch (error) {
        console.error('Error loading work center assignments:', error);
        list.innerHTML = `<li><span class="text-red-500">${error.message}</span></li>`;
    }
}

function closeWorkCenterAssignmentModal() {
    document.getElementById('workCenterAssignmentModal').classList.add('hidden');
    selectedUserId = null;
}

async function toggleWorkCenterAssignment(userId, workCenterId, assign) {
    const headers = getAuthHeaders();
    const url = `/api/account/users/${userId}/workcenters/${workCenterId}`;
    const method = assign ? 'POST' : 'DELETE';

    try {
        const response = await fetch(url, { method, headers });
        if (!response.ok) {
            throw new Error('Failed to update assignment');
        }
        // Optionally, show a success message or just rely on the UI updating
        console.log(`Assignment for WC ${workCenterId} to User ${userId} updated.`);
    } catch (error) {
        console.error('Error toggling work center assignment:', error);
        alert('Failed to update work center assignment.');
        // Re-open the modal to show the real state of assignments
        openWorkCenterAssignmentModal(userId);
    }
}

// Initial load
document.addEventListener('DOMContentLoaded', () => {
    loadUsers();
    loadWorkCenters();
}); 