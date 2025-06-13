// Check authentication
function checkAuth() {
    // Session-based authentication kullanıyoruz, token kontrolü gereksiz
    loadUserInfo();
}

// Load user info and setup navigation
async function loadUserInfo() {
    try {
        const response = await fetch('/api/account/users/me', {
            method: 'GET',
            credentials: 'include', // Session cookies için gerekli
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            if (response.status === 401) {
                logout();
                return;
            }
            throw new Error(t('Failed to load user info'));
        }

        const user = await response.json();
        setupNavigation(user);
        displayUserInfo(user);
    } catch (error) {
        console.error('Error loading user info:', error);
    }
}

// Setup navigation based on user role
function setupNavigation(user) {
    const navLinks = document.getElementById('navLinks');
    navLinks.innerHTML = '';

    // Common links
    const links = [
        { text: t('Orders'), href: '/orders.html', show: true }
    ];

    // Admin-only links
    if (user.isAdmin) {
        links.push({ text: t('Admin Panel'), href: '/admin.html', show: true });
    }

    links.forEach(link => {
        if (link.show) {
            const a = document.createElement('a');
            a.href = link.href;
            a.className = 'inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium';
            
            // Highlight current page
            if (window.location.pathname === link.href) {
                a.classList.add('border-blue-500', 'text-gray-900');
            } else {
                a.classList.add('border-transparent', 'text-gray-500', 'hover:border-gray-300', 'hover:text-gray-700');
            }
            
            a.textContent = link.text;
            navLinks.appendChild(a);
        }
    });
}

// Display user info in header
function displayUserInfo(user) {
    const userInfo = document.getElementById('userInfo');
    userInfo.textContent = `${user.fullName} (${user.username})`;
}

// Logout function
function logout() {
    // Session-based logout için sadece login sayfasına yönlendir
    window.location.href = '/login.html';
}

// Initialize
document.addEventListener('DOMContentLoaded', checkAuth); 