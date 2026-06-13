import { Navbar } from '../components/layout/Navbar';
import { Footer } from '../components/layout/Footer';
import { HeroSection } from '../components/landing/HeroSection';
import { StatsSection } from '../components/landing/StatsSection';
import { FeaturesSection } from '../components/landing/FeaturesSection';
import { RoleExperienceSection } from '../components/landing/RoleExperienceSection';
import { FeaturedTournamentSection } from '../components/landing/FeaturedTournamentSection';
import { HowItWorksSection } from '../components/landing/HowItWorksSection';
import { CTASection } from '../components/landing/CTASection';

export function HomePage() {
  return (
    <div className="min-h-screen bg-navy text-body font-sans selection:bg-gold/30 selection:text-white">
      <Navbar />
      <main>
        <HeroSection />
        <StatsSection />
        <FeaturesSection />
        <RoleExperienceSection />
        <FeaturedTournamentSection />
        <HowItWorksSection />
        <CTASection />
      </main>
      <Footer />
    </div>
  );
}
