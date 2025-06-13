const translations = {
    // Common
    'Loading': 'Yükleniyor...',
    'Save': 'Kaydet',
    'Cancel': 'İptal',
    'Delete': 'Sil',
    'Edit': 'Düzenle',
    'View': 'Görüntüle',
    'Close': 'Kapat',
    'Yes': 'Evet',
    'No': 'Hayır',
    'Error': 'Hata',
    'Success': 'Başarılı',
    'Warning': 'Uyarı',
    'Required': 'Zorunlu alan',

    // Login
    'Sign in to ERP Portal': 'ERP Portal\'a Giriş',
    'Username': 'Kullanıcı Adı',
    'Password': 'Şifre',
    'Sign in': 'Giriş Yap',
    'Login failed': 'Giriş başarısız',
    'Please check your credentials': 'Lütfen bilgilerinizi kontrol edin',

    // Navigation
    'Orders': 'Siparişler',
    'Admin Panel': 'Yönetici Paneli',
    'Logout': 'Çıkış',

    // Orders
    'Shop Orders': 'Atölye Siparişleri',
    'All Work Centers': 'Tüm İş Merkezleri',
    'New Order': 'Yeni Sipariş',
    'Order No': 'Sipariş No',
    'Work Center': 'İş Merkezi',
    'Status': 'Durum',
    'Created Date': 'Oluşturma Tarihi',
    'Actions': 'İşlemler',
    'Description': 'Açıklama',
    'Select Work Center': 'İş Merkezi Seçin',
    'Order Details': 'Sipariş Detayları',
    'No description': 'Açıklama yok',
    'Are you sure you want to delete this order?': 'Bu siparişi silmek istediğinizden emin misiniz?',
    'Failed to load orders': 'Siparişler yüklenemedi',
    'Failed to load work centers': 'İş merkezleri yüklenemedi',
    'Failed to save order': 'Sipariş kaydedilemedi',
    'Failed to delete order': 'Sipariş silinemedi',
    'Failed to load order details': 'Sipariş detayları yüklenemedi',

    // Admin
    'User Management': 'Kullanıcı Yönetimi',
    'Work Center Management': 'İş Merkezi Yönetimi',
    'Add User': 'Kullanıcı Ekle',
    'Add Work Center': 'İş Merkezi Ekle',
    'Full Name': 'Ad Soyad',
    'Admin': 'Yönetici',
    'Work Centers': 'İş Merkezleri',
    'Code': 'Kod',
    'Name': 'Ad',
    'Active': 'Aktif',
    'Manage': 'Yönet',
    'Manage Work Centers': 'İş Merkezlerini Yönet',

    // Status
    'New': 'Yeni',
    'InProgress': 'Devam Ediyor',
    'Completed': 'Tamamlandı',
    'Cancelled': 'İptal Edildi',
    'Released': 'Yayımlanmış',
    'InProcess': 'İşlemde',
    'Interruption': 'Kesinti',
    'PartiallyReported': 'Kısmi Raporlama',
    'Closed': 'Kapalı'
};

// Translation function
function t(key) {
    return translations[key] || key;
}

// Export for use in other files
export { t }; 