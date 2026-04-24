---
name: Inventory Precision
colors:
  surface: '#f7f9fb'
  surface-dim: '#d8dadc'
  surface-bright: '#f7f9fb'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f2f4f6'
  surface-container: '#eceef0'
  surface-container-high: '#e6e8ea'
  surface-container-highest: '#e0e3e5'
  on-surface: '#191c1e'
  on-surface-variant: '#434655'
  inverse-surface: '#2d3133'
  inverse-on-surface: '#eff1f3'
  outline: '#737686'
  outline-variant: '#c3c6d7'
  surface-tint: '#0053db'
  primary: '#004ac6'
  on-primary: '#ffffff'
  primary-container: '#2563eb'
  on-primary-container: '#eeefff'
  inverse-primary: '#b4c5ff'
  secondary: '#505f76'
  on-secondary: '#ffffff'
  secondary-container: '#d0e1fb'
  on-secondary-container: '#54647a'
  tertiary: '#943700'
  on-tertiary: '#ffffff'
  tertiary-container: '#bc4800'
  on-tertiary-container: '#ffede6'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dbe1ff'
  primary-fixed-dim: '#b4c5ff'
  on-primary-fixed: '#00174b'
  on-primary-fixed-variant: '#003ea8'
  secondary-fixed: '#d3e4fe'
  secondary-fixed-dim: '#b7c8e1'
  on-secondary-fixed: '#0b1c30'
  on-secondary-fixed-variant: '#38485d'
  tertiary-fixed: '#ffdbcd'
  tertiary-fixed-dim: '#ffb596'
  on-tertiary-fixed: '#360f00'
  on-tertiary-fixed-variant: '#7d2d00'
  background: '#f7f9fb'
  on-background: '#191c1e'
  surface-variant: '#e0e3e5'
typography:
  headline-lg:
    fontFamily: Manrope
    fontSize: 30px
    fontWeight: '700'
    lineHeight: 38px
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Manrope
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
    letterSpacing: -0.01em
  headline-sm:
    fontFamily: Manrope
    fontSize: 18px
    fontWeight: '600'
    lineHeight: 26px
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  label-md:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.05em
  code-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  base: 4px
  xs: 4px
  sm: 8px
  md: 16px
  lg: 24px
  xl: 40px
  container-max: 1280px
  gutter: 20px
---

## Brand & Style

This design system is built for a high-efficiency item management environment where clarity and speed of recognition are paramount. The brand personality is professional, systematic, and reliable, aiming to reduce cognitive load for users managing complex inventories.

The chosen style is **Corporate / Modern** with a lean toward **Minimalism**. It prioritizes a high signal-to-noise ratio by utilizing generous white space and a structured information hierarchy. The emotional response should be one of "controlled organization"—where the interface feels like a powerful tool that stays out of the user's way. Focus is placed on data legibility, clear state indicators, and a predictable interface flow.

## Colors

The palette is anchored by a "Precision Blue" primary color used exclusively for intent-driven actions and active states. To maintain a soft, non-distracting environment, the background utilizes a series of cool-toned grays that differentiate functional areas without harsh lines.

- **Primary (#2563EB):** Reserved for primary buttons, selection states, and critical progress indicators.
- **Secondary (#64748B):** Used for supporting text, icons, and non-primary actions.
- **Backgrounds:** Use `#F8FAFC` for the main application canvas and `#FFFFFF` for content cards and elevated surfaces to create subtle contrast.
- **Semantic Colors:** Green and Red are used sparingly for stock status (In-Stock vs. Out-of-Stock) and error validation.

## Typography

The typography system uses a pairing of **Manrope** for structural headings and **Inter** for data-heavy body content. 

- **Manrope** provides a modern, refined look for page titles and section headers, lending a sense of contemporary professionalism.
- **Inter** is utilized for its exceptional legibility in tabular data and item lists. 
- **Labels** (label-md) should be used for table headers and metadata categories, utilizing a slight tracking increase and uppercase transform to distinguish them from editable data.

## Layout & Spacing

The layout follows a **Fixed Grid** philosophy for desktop views to ensure data density remains manageable and readable, centered on a 1280px maximum container. 

- **Rhythm:** A 4px baseline grid governs all spacing. 
- **Grid:** A 12-column grid is used for dashboard layouts. Item detail pages should use a "Large-Small" split (8 columns for main info, 4 columns for metadata/sidebar).
- **Density:** Use "Compact" spacing (8px) for list items and tables to allow more data visibility, and "Standard" spacing (16px-24px) for form layouts and general padding to ensure touch targets are accessible.

## Elevation & Depth

This design system utilizes **Tonal Layers** supplemented by **Low-contrast outlines** to define depth, avoiding heavy shadows to keep the interface feeling "light."

1.  **Level 0 (Background):** `#F8FAFC` - The base canvas.
2.  **Level 1 (Cards/Surface):** `#FFFFFF` with a 1px border of `#E2E8F0`. No shadow.
3.  **Level 2 (Active/Hover):** `#FFFFFF` with a subtle ambient shadow (0px 4px 12px rgba(0,0,0,0.05)).
4.  **Level 3 (Modals/Popovers):** `#FFFFFF` with a defined shadow (0px 10px 25px rgba(0,0,0,0.1)) to focus user attention.

Interactive elements should use a subtle inset shadow on 'click' to provide tactile feedback without breaking the minimalist aesthetic.

## Shapes

The shape language is **Soft**, striking a balance between the rigid efficiency of a warehouse/database and a modern user-friendly application.

- **Standard Elements (Buttons, Inputs):** 0.25rem (4px) corner radius. This provides a professional, "to-the-point" appearance.
- **Containers (Cards, Modals):** 0.5rem (8px) corner radius to soften large blocks of content.
- **Status Tags:** 1rem (16px) pill-shape to distinguish status indicators from clickable buttons or input fields.

## Components

- **Buttons:** Primary buttons use a solid blue fill with white text. Secondary buttons use a transparent background with a gray border. Use 44px height for main actions and 32px for in-row table actions.
- **Inputs:** Use a 1px border (`#D1D5DB`). On focus, the border transitions to the primary blue with a 2px outer glow. Labels are always positioned above the input field for maximum scanability.
- **Item Cards:** Should include a thumbnail placeholder, title (headline-sm), and secondary metadata (body-md). Use a 1px border to define the card boundaries.
- **Status Chips:** Use low-saturation background tints (e.g., light green background with dark green text) for status indicators like "In Stock" or "Pending."
- **Data Tables:** Use zebra-striping (alternating `#F8FAFC` and `#FFFFFF`) only for very wide tables. Default tables should use simple dividers (`#E2E8F0`).
- **Search Bar:** A prominent component featuring a "Search" icon prefix and a keyboard shortcut hint (e.g., "⌘K") to emphasize the functional, power-user nature of the app.