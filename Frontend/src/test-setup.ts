import { beforeAll, afterEach } from 'vitest';

// src/test-setup.ts
import { TestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';

// Initialise Angular TestBed before any spec runs.
beforeAll(() => {
  TestBed.initTestEnvironment(
    BrowserDynamicTestingModule,
    platformBrowserDynamicTesting()
  );
});

// Reset TestBed after each spec to avoid cross-test contamination.
afterEach(() => {
  TestBed.resetTestingModule();
});
