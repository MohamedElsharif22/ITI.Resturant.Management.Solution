
// Update business hours status
function updateBusinessStatus() {
const now = new Date();
const hours = now.getHours();
const statusElement = document.getElementById('businessStatus');

if (hours >= 9 && hours < 23) {
    statusElement.textContent = 'Open Now';
    statusElement.parentElement.style.background = 'linear-gradient(135deg, #2eb649, #4fd96b)';
} else {
    statusElement.textContent = 'Closed';
    statusElement.parentElement.style.background = 'linear-gradient(135deg, #d9534f, #c9302c)';
}
}

updateBusinessStatus();
setInterval(updateBusinessStatus, 60000); // Update every minute

// Highlight active nav link
const currentPath = window.location.pathname;
document.querySelectorAll('.nav-link').forEach(link => {
if (link.getAttribute('href') === currentPath) {
    link.classList.add('active');
}
});


// Add smooth scroll behavior
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({ behavior: 'smooth' });
        }
    });
});

// Animate stats on scroll
const observerOptions = {
    threshold: 0.5,
    rootMargin: '0px 0px -100px 0px'
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.style.animation = 'fadeInUp 0.6s ease forwards';
        }
    });
}, observerOptions);

document.querySelectorAll('.feature-card, .action-card').forEach(card => {
    observer.observe(card);
});