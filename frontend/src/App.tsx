import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import OrgChart from './components/OrgChart';
import './App.css';

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 3,
      refetchOnWindowFocus: false,
      staleTime: 1000 * 60 * 5, // 5 minutes
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <div className="App">
        <OrgChart />
      </div>
    </QueryClientProvider>
  );
}

export default App;