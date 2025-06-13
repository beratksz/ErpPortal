async function handleLogin(event) {
    event.preventDefault();
    
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    try {
        const response = await fetch('/api/account/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ username, password })
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Invalid credentials');
        }

        const data = await response.json();
        
        // Store the JWT token
        localStorage.setItem('token', data.token);
        localStorage.setItem('user', JSON.stringify(data.user));

        // Redirect based on user role and work center assignments
        if (data.user.isAdmin) {
            window.location.href = '/admin.html';
        } else {
            if (data.user.workCenters && data.user.workCenters.length === 1) {
                // Auto-select the only work center and go to orders
                localStorage.setItem('selectedWorkCenter', data.user.workCenters[0].code);
                window.location.href = '/orders.html';
            } else if (data.user.workCenters && data.user.workCenters.length > 1) {
                // More than one work center, let the user choose
                window.location.href = '/select-workcenter.html';
            } else {
                // No assigned work centers, show an error
                alert('You are not assigned to any work center. Please contact an administrator.');
                localStorage.removeItem('token');
                localStorage.removeItem('user');
            }
        }
    } catch (error) {
        console.error('Login error:', error);
        alert(error.message || 'Login failed. Please check your credentials and try again.');
    }
}