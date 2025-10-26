import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { TamaguiProvider } from '@tamagui/core'
import { tamaguiConfig } from '../tamagui.config'
import './index.css'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <TamaguiProvider config={tamaguiConfig}>
      <App />
    </TamaguiProvider>
  </StrictMode>,
)
