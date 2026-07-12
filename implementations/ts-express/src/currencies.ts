/**
 * The 49 supported ISO 4217 alphabetic currency codes, matching the shared contract.
 * Currency is validated against this set only when an article's price is greater than 0.
 */
export const SUPPORTED_CURRENCIES: ReadonlySet<string> = new Set<string>([
  // Major currencies
  'USD', 'EUR', 'JPY', 'GBP', 'CHF', 'CAD', 'AUD', 'NZD',
  // European currencies
  'SEK', 'NOK', 'DKK', 'CZK', 'PLN', 'HUF', 'RON', 'BGN', 'ISK',
  // Asian currencies
  'CNY', 'HKD', 'SGD', 'KRW', 'INR', 'THB', 'MYR', 'IDR', 'PHP', 'TWD', 'VND',
  // Latin American currencies
  'BRL', 'MXN', 'ARS', 'CLP', 'COP', 'PEN',
  // Middle Eastern currencies
  'AED', 'SAR', 'ILS', 'QAR', 'KWD', 'BHD',
  // Other currencies
  'ZAR', 'TRY', 'RUB', 'UAH', 'EGP', 'NGN', 'KES', 'MAD', 'PKR',
]);

export function isSupportedCurrency(code: string | null | undefined): boolean {
  return typeof code === 'string' && SUPPORTED_CURRENCIES.has(code);
}
