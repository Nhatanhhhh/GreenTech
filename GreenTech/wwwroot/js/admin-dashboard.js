// Admin Dashboard JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Initialize all dashboard features
    initSidebarToggle();
    initDropdowns();
    initCharts();
    initSearch();
    initNotifications();
    initQuickActions();
});

// Sidebar Toggle for Mobile
function initSidebarToggle() {
    const sidebarToggle = document.getElementById('sidebar-toggle');
    const sidebar = document.getElementById('sidebar');
    const sidebarOverlay = document.getElementById('sidebar-overlay');
    const sidebarClose = document.getElementById('sidebar-close');

    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('-translate-x-full');
            sidebarOverlay.classList.toggle('hidden');
            document.body.classList.toggle('overflow-hidden');
        });
    }

    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', function() {
            sidebar.classList.add('-translate-x-full');
            sidebarOverlay.classList.add('hidden');
            document.body.classList.remove('overflow-hidden');
        });
    }

    if (sidebarClose) {
        sidebarClose.addEventListener('click', function() {
            sidebar.classList.add('-translate-x-full');
            sidebarOverlay.classList.add('hidden');
            document.body.classList.remove('overflow-hidden');
        });
    }

    // Close sidebar when clicking outside on mobile
    document.addEventListener('click', function(e) {
        if (window.innerWidth < 1024) {
            if (!sidebar.contains(e.target) && !sidebarToggle.contains(e.target)) {
                sidebar.classList.add('-translate-x-full');
                sidebarOverlay.classList.add('hidden');
                document.body.classList.remove('overflow-hidden');
            }
        }
    });
}

// Initialize Dropdowns
function initDropdowns() {
    // Notification Dropdown
    const notificationButton = document.getElementById('notification-button');
    const notificationDropdown = document.getElementById('notification-dropdown');

    if (notificationButton && notificationDropdown) {
        notificationButton.addEventListener('click', function(e) {
            e.stopPropagation();
            notificationDropdown.classList.toggle('hidden');
            
            // Close other dropdowns
            const userDropdown = document.getElementById('user-dropdown');
            if (userDropdown) userDropdown.classList.add('hidden');
        });
    }

    // User Menu Dropdown
    const userMenuButton = document.getElementById('user-menu-button');
    const userDropdown = document.getElementById('user-dropdown');

    if (userMenuButton && userDropdown) {
        userMenuButton.addEventListener('click', function(e) {
            e.stopPropagation();
            userDropdown.classList.toggle('hidden');
            
            // Close other dropdowns
            if (notificationDropdown) notificationDropdown.classList.add('hidden');
        });
    }

    // Close dropdowns when clicking outside
    document.addEventListener('click', function(e) {
        if (notificationDropdown && !notificationButton.contains(e.target) && !notificationDropdown.contains(e.target)) {
            notificationDropdown.classList.add('hidden');
        }
        if (userDropdown && !userMenuButton.contains(e.target) && !userDropdown.contains(e.target)) {
            userDropdown.classList.add('hidden');
        }
    });
}

// Initialize Charts
function initCharts() {
    // Sales Chart
    const salesCtx = document.getElementById('salesChart');
    if (salesCtx && typeof Chart !== 'undefined') {
        new Chart(salesCtx, {
            type: 'line',
            data: {
                labels: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6'],
                datasets: [{
                    label: 'Doanh thu',
                    data: [12000000, 19000000, 15000000, 25000000, 22000000, 30000000],
                    borderColor: 'rgb(34, 197, 94)',
                    backgroundColor: 'rgba(34, 197, 94, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function(value) {
                                return new Intl.NumberFormat('vi-VN', {
                                    style: 'currency',
                                    currency: 'VND',
                                    notation: 'compact'
                                }).format(value);
                            }
                        }
                    }
                },
                interaction: {
                    intersect: false,
                    mode: 'index'
                }
            }
        });
    }

    // Orders Chart
    const ordersCtx = document.getElementById('ordersChart');
    if (ordersCtx && typeof Chart !== 'undefined') {
        new Chart(ordersCtx, {
            type: 'doughnut',
            data: {
                labels: ['Hoàn thành', 'Đang xử lý', 'Đã hủy'],
                datasets: [{
                    data: [65, 25, 10],
                    backgroundColor: [
                        'rgb(34, 197, 94)',
                        'rgb(251, 191, 36)',
                        'rgb(239, 68, 68)'
                    ],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 20,
                            usePointStyle: true
                        }
                    }
                }
            }
        });
    }
}

// Initialize Search
function initSearch() {
    const searchInput = document.querySelector('input[type="text"][placeholder*="Tìm kiếm"]');
    if (searchInput) {
        let searchTimeout;
        
        searchInput.addEventListener('input', function(e) {
            clearTimeout(searchTimeout);
            const query = e.target.value.trim();
            
            if (query.length > 2) {
                searchTimeout = setTimeout(() => {
                    performSearch(query);
                }, 300);
            }
        });

        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                const query = e.target.value.trim();
                if (query) {
                    performSearch(query);
                }
            }
        });
    }
}

// Perform Search
function performSearch(query) {
    console.log('Searching for:', query);
    // Implement search functionality here
    // This could make an AJAX call to your search endpoint
}

// Initialize Notifications
function initNotifications() {
    const notificationButton = document.getElementById('notification-button');
    const notificationBadge = notificationButton?.querySelector('span');
    
    if (notificationBadge) {
        // Simulate real-time notifications
        setInterval(() => {
            const currentCount = parseInt(notificationBadge.textContent) || 0;
            if (Math.random() > 0.8) { // 20% chance every 10 seconds
                notificationBadge.textContent = currentCount + 1;
                notificationBadge.classList.add('animate-pulse');
                setTimeout(() => {
                    notificationBadge.classList.remove('animate-pulse');
                }, 1000);
            }
        }, 10000);
    }
}

// Initialize Quick Actions
function initQuickActions() {
    const quickActionButtons = document.querySelectorAll('[title="Thêm sản phẩm"], [title="Báo cáo"]');
    
    quickActionButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const action = this.getAttribute('title');
            
            switch(action) {
                case 'Thêm sản phẩm':
                    showAddProductModal();
                    break;
                case 'Báo cáo':
                    showReportsModal();
                    break;
            }
        });
    });
}

// Show Add Product Modal
function showAddProductModal() {
    // Create modal HTML
    const modal = document.createElement('div');
    modal.className = 'fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4';
    modal.innerHTML = `
        <div class="bg-white rounded-xl shadow-xl max-w-md w-full p-6">
            <div class="flex items-center justify-between mb-4">
                <h3 class="text-lg font-semibold text-gray-900">Thêm sản phẩm mới</h3>
                <button class="text-gray-400 hover:text-gray-600" onclick="this.closest('.fixed').remove()">
                    <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </button>
            </div>
            <form class="space-y-4">
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Tên sản phẩm</label>
                    <input type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500">
                </div>
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Giá</label>
                    <input type="number" class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500">
                </div>
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Mô tả</label>
                    <textarea class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-green-500" rows="3"></textarea>
                </div>
                <div class="flex gap-3 pt-4">
                    <button type="button" class="flex-1 px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200" onclick="this.closest('.fixed').remove()">
                        Hủy
                    </button>
                    <button type="submit" class="flex-1 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700">
                        Thêm sản phẩm
                    </button>
                </div>
            </form>
        </div>
    `;
    
    document.body.appendChild(modal);
}

// Show Reports Modal
function showReportsModal() {
    const modal = document.createElement('div');
    modal.className = 'fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4';
    modal.innerHTML = `
        <div class="bg-white rounded-xl shadow-xl max-w-2xl w-full p-6">
            <div class="flex items-center justify-between mb-4">
                <h3 class="text-lg font-semibold text-gray-900">Báo cáo</h3>
                <button class="text-gray-400 hover:text-gray-600" onclick="this.closest('.fixed').remove()">
                    <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </button>
            </div>
            <div class="grid grid-cols-2 gap-4">
                <button class="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 text-left">
                    <div class="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center mb-2">
                        <svg class="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
                        </svg>
                    </div>
                    <h4 class="font-medium text-gray-900">Báo cáo doanh thu</h4>
                    <p class="text-sm text-gray-500">Xem chi tiết doanh thu theo thời gian</p>
                </button>
                <button class="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 text-left">
                    <div class="w-8 h-8 bg-green-100 rounded-lg flex items-center justify-center mb-2">
                        <svg class="w-4 h-4 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"></path>
                        </svg>
                    </div>
                    <h4 class="font-medium text-gray-900">Báo cáo đơn hàng</h4>
                    <p class="text-sm text-gray-500">Thống kê đơn hàng và trạng thái</p>
                </button>
                <button class="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 text-left">
                    <div class="w-8 h-8 bg-purple-100 rounded-lg flex items-center justify-center mb-2">
                        <svg class="w-4 h-4 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"></path>
                        </svg>
                    </div>
                    <h4 class="font-medium text-gray-900">Báo cáo sản phẩm</h4>
                    <p class="text-sm text-gray-500">Phân tích hiệu suất sản phẩm</p>
                </button>
                <button class="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 text-left">
                    <div class="w-8 h-8 bg-orange-100 rounded-lg flex items-center justify-center mb-2">
                        <svg class="w-4 h-4 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path>
                        </svg>
                    </div>
                    <h4 class="font-medium text-gray-900">Báo cáo khách hàng</h4>
                    <p class="text-sm text-gray-500">Thống kê khách hàng và hành vi</p>
                </button>
            </div>
            <div class="flex gap-3 pt-4">
                <button class="flex-1 px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200" onclick="this.closest('.fixed').remove()">
                    Đóng
                </button>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
}

// Utility Functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `fixed top-4 right-4 z-50 px-6 py-3 rounded-lg shadow-lg text-white ${
        type === 'success' ? 'bg-green-500' : 
        type === 'error' ? 'bg-red-500' : 
        'bg-blue-500'
    }`;
    toast.textContent = message;
    
    document.body.appendChild(toast);
    
    setTimeout(() => {
        toast.remove();
    }, 3000);
}

// Export functions for global access
window.AdminDashboard = {
    showToast,
    formatCurrency,
    showAddProductModal,
    showReportsModal
};
