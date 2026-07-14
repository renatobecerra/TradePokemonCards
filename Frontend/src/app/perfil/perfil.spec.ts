import { describe, it, expect, vi, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { PerfilComponent } from './perfil';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

describe('PerfilComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        RouterTestingModule,
        FormsModule,
        ReactiveFormsModule,
        PerfilComponent
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    })
    .overrideTemplate(PerfilComponent, '<div></div>')
    .compileComponents();
  });

  it('debe crearse correctamente', () => {
    const fixture = TestBed.createComponent(PerfilComponent);
    const component = fixture.componentInstance;
    expect(component).toBeTruthy();
  });
});
