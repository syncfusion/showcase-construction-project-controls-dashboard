import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { registerLicense } from '@syncfusion/ej2-base';
import { RouterProvider } from 'react-router-dom';
import { router } from './routes';
import './styles/global.css';

const licenseKey = import.meta.env.VITE_SYNCFUSION_LICENSE_KEY as string | undefined;
if (licenseKey) {
  registerLicense(licenseKey);
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
);
