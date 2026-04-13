<script setup lang="ts">
import { ref } from 'vue'

const currentYear = ref(new Date().getFullYear())
</script>

<template>
  <footer class="footer">
    <div class="footer-content">
      <div class="footer-section">
        <h3 class="footer-title">SportReservation</h3>
        <p class="footer-description">Pronajmujte sportoviště snadno a bezpečně. Vaše oblíbená platforma pro rezervaci sportovních zařízení.</p>
        <div class="social-links">
          <a href="#" class="social-link" aria-label="Facebook">
            <i class="mdi mdi-facebook"></i>
          </a>
          <a href="#" class="social-link" aria-label="Instagram">
            <i class="mdi mdi-instagram"></i>
          </a>
          <a href="#" class="social-link" aria-label="Twitter">
            <i class="mdi mdi-twitter"></i>
          </a>
          <a href="#" class="social-link" aria-label="LinkedIn">
            <i class="mdi mdi-linkedin"></i>
          </a>
        </div>
      </div>

      <div class="footer-section">
        <h4 class="section-title">Nápověda</h4>
        <ul class="link-list">
          <li><a href="#">Jak funguje</a></li>
          <li><a href="#">Jejich pravidla</a></li>
          <li><a href="#">Bezpečnost</a></li>
          <li><a href="#">Podpora</a></li>
        </ul>
      </div>

      <div class="footer-section">
        <h4 class="section-title">Společnost</h4>
        <ul class="link-list">
          <li><a href="#">O nás</a></li>
          <li><a href="#">Pracovních míst</a></li>
          <li><a href="#">Blog</a></li>
          <li><a href="#">Kontakt</a></li>
        </ul>
      </div>

      <div class="footer-section">
        <h4 class="section-title">Právní</h4>
        <ul class="link-list">
          <li><a href="#">Podmínky služby</a></li>
          <li><a href="#">Ochrana osobních údajů</a></li>
          <li><a href="#">Cookies</a></li>
          <li><a href="#">Právní upozornění</a></li>
        </ul>
      </div>
    </div>

    <div class="footer-divider"></div>

    <div class="footer-bottom">
      <p class="footer-copyright">&copy; {{ currentYear }} SportReservation. Všechna práva vyhrazena.</p>
      <p class="footer-meta">Postaveno s <span class="heart">❤</span> pro sport.</p>
    </div>
  </footer>
</template>

<style scoped>
.footer {
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
  color: #e0e0e0;
  padding: 3rem 1.5rem 1rem;
  margin-top: 5rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.footer-content {
  max-width: 1200px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  gap: 2rem;
  margin-bottom: 2rem;
}

.footer-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.footer-title {
  font-family: 'Baloo 2', sans-serif;
  font-size: 1.5rem;
  font-weight: 700;
  background: linear-gradient(135deg, #1E88E5, #00BCD4);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0;
}

.footer-description {
  font-size: 0.95rem;
  line-height: 1.6;
  color: #b0b0b0;
  margin: 0;
}

.social-links {
  display: flex;
  gap: 0.75rem;
  margin-top: 0.5rem;
}

.social-link {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  background: rgba(30, 136, 229, 0.15);
  border: 2px solid rgba(30, 136, 229, 0.3);
  border-radius: 50%;
  color: #1E88E5;
  text-decoration: none;
  transition: all 0.3s ease;
  font-size: 1.2rem;
}

.social-link:hover {
  background: rgba(30, 136, 229, 0.3);
  border-color: #1E88E5;
  transform: translateY(-3px);
  color: #00BCD4;
}

.section-title {
  font-family: 'Baloo 2', sans-serif;
  font-size: 1rem;
  font-weight: 700;
  color: #ffffff;
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.link-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.link-list a {
  color: #b0b0b0;
  text-decoration: none;
  transition: all 0.3s ease;
  font-size: 0.95rem;
}

.link-list a:hover {
  color: #1E88E5;
  transform: translateX(4px);
}

.footer-divider {
  max-width: 1200px;
  margin: 2rem auto;
  height: 1px;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
}

.footer-bottom {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
  text-align: center;
}

.footer-copyright,
.footer-meta {
  margin: 0;
  font-size: 0.9rem;
  color: #808080;
}

.heart {
  color: #E53935;
  animation: heartbeat 0.6s ease-in-out infinite;
}

@keyframes heartbeat {
  0%, 100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.2);
  }
}

/* Responsive Design */
@media (max-width: 1024px) {
  .footer-content {
    grid-template-columns: 1.5fr 1fr 1fr;
    gap: 1.5rem;
  }
}

@media (max-width: 768px) {
  .footer {
    padding: 2rem 1rem 1rem;
    margin-top: 3rem;
  }

  .footer-content {
    grid-template-columns: 1fr 1fr;
    gap: 1.5rem;
    margin-bottom: 1.5rem;
  }

  .footer-bottom {
    flex-direction: column;
    justify-content: center;
  }

  .section-title {
    font-size: 0.9rem;
  }

  .link-list a {
    font-size: 0.9rem;
  }
}

@media (max-width: 480px) {
  .footer {
    padding: 1.5rem 1rem 0.75rem;
  }

  .footer-content {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }

  .social-link {
    width: 36px;
    height: 36px;
    font-size: 1rem;
  }

  .footer-title {
    font-size: 1.25rem;
  }

  .footer-description {
    font-size: 0.9rem;
  }
}
</style>
