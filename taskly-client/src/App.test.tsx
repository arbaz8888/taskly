import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import App from './App';

// Mock fetch for /tasks/ call
const tasksResponse = [{ id: 1, title: 'Hello from test', isComplete: false, workspaceId: 1 }];

beforeEach(() => {
  // Fake token so App shows <Tasks/>
  window.localStorage.setItem('taskly_token', 'fake-jwt');

  // Strongly typed mock for fetch (no "any")
  const mockFetch: typeof fetch = (input: RequestInfo | URL) => {
    const url = typeof input === 'string' ? input : input.toString();
    const init: ResponseInit = {
      status: 200,
      headers: { 'Content-Type': 'application/json' }
    };

    if (url.includes('/tasks/')) {
      return Promise.resolve(new Response(JSON.stringify(tasksResponse), init));
    }
    return Promise.resolve(new Response('{}', init));
  };

  vi.spyOn(globalThis, 'fetch').mockImplementation(mockFetch);
});


describe('App tasks list', () => {
  it('renders a task item from API', async () => {
    render(<App />);
    await waitFor(() => expect(screen.getByText('Hello from test')).toBeInTheDocument());
  });
});
