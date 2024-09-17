/** @type {import('next').NextConfig} */
const nextConfig = {
  experimental: {
    serverActions: true
  },
    images: {
        remotePatterns: [
            {
              protocol: 'https',
              hostname: 'cdn.pixabay.com',
              pathname: '**',
            },
          ],
    }
};

export default nextConfig;
