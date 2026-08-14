import { vi } from 'vitest'

const createMatchMediaMock = () => ({
  matches: false,
  media: '',
  onchange: null,
  addListener: vi.fn(),
  removeListener: vi.fn(),
  addEventListener: vi.fn(),
  removeEventListener: vi.fn(),
  dispatchEvent: vi.fn(),
})

Object.defineProperties(window, {
  scrollTo: {
    value: vi.fn(),
    writable: true,
    configurable: true,
  },
  scrollBy: {
    value: vi.fn(),
    writable: true,
    configurable: true,
  },
  matchMedia: {
    value: vi.fn().mockImplementation(createMatchMediaMock),
    writable: true,
    configurable: true,
  },
})

Object.defineProperty(globalThis, 'scrollTo', {
  value: window.scrollTo,
  writable: true,
  configurable: true,
})

Object.defineProperty(HTMLElement.prototype, 'scrollIntoView', {
  value: vi.fn(),
  writable: true,
  configurable: true,
})
