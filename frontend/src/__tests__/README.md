# Frontend Test Documentation

## Test Structure

### Unit Tests
- **Types**: `src/types/__tests__/` - Type definition tests
- **Services**: `src/services/__tests__/` - API and hook tests
- **Components**: `src/components/__tests__/` - React component tests
- **App**: `src/App.test.tsx` - Main application tests

### Test Files Created
1. `App.test.tsx` - Main app component tests
2. `types/__tests__/orgchart.test.ts` - Type validation tests
3. `services/__tests__/api.test.ts` - API client tests
4. `services/__tests__/hooks.test.tsx` - React Query hooks tests
5. `components/__tests__/OrgChart.test.tsx` - Main component tests

### Setup Files
- `setupTests.ts` - Jest configuration and mocks

## Running Tests

```bash
# Run all tests
npm test

# Run tests without watch mode
npm test -- --watchAll=false

# Run specific test file
npm test -- App.test.tsx

# Run with coverage
npm test -- --coverage --watchAll=false
```

## Test Coverage

Current test coverage includes:
- ✅ Component rendering
- ✅ Loading states
- ✅ Error handling
- ✅ User interactions (filters, zoom)
- ✅ API integration
- ✅ Type definitions
- ✅ React Query hooks

## Mocking Strategy

- **API calls**: Mocked using Jest mocks
- **React Query**: Wrapped with QueryClientProvider
- **Browser APIs**: ResizeObserver, requestAnimationFrame mocked
- **External dependencies**: Mocked at module level

## Future Enhancements

Consider adding:
- E2E tests with Playwright or Cypress
- Visual regression tests
- Performance tests
- Accessibility tests