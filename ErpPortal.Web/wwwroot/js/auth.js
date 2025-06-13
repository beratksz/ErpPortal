// Auth module
const auth = {
    currentWorkCenter: '',
    
    async login(username, workCenterCode) {
        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password: workCenterCode })
            });

            if (response.ok) {
                const data = await response.json();
                this.currentWorkCenter = data.workCenter;
                return { success: true, data };
            } else {
                return { 
                    success: false, 
                    error: 'Invalid username or work center code' 
                };
            }
        } catch (error) {
            console.error('Login error:', error);
            return { 
                success: false, 
                error: 'An error occurred. Please try again.' 
            };
        }
    }
};

export default auth; 