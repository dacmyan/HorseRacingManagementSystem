// Các hàm tiện ích dùng chung cho toàn app

export function formatCurrency(amount, currency = 'USD') {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency,
    maximumFractionDigits: 0,
  }).format(amount);
}

export function formatCurrencyVND(amount) {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(amount);
}

export function formatDate(date) {
  return new Intl.DateTimeFormat('vi-VN', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(date));
}

export function formatRaceCountdown(targetDate) {
  const diff = new Date(targetDate) - Date.now();
  if (diff <= 0) return 'Ended';
  const days = Math.floor(diff / (1000 * 60 * 60 * 24));
  const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
  return days > 0 ? `${days}d ${hours}h` : `${hours}h`;
}

export function formatPercentage(value, total) {
  if (!total) return '0%';
  return `${Math.round((value / total) * 100)}%`;
}
