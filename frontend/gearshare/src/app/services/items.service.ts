import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ItemDto, CreateItemRequest, UpdateItemRequest } from '../models/item.model';

@Injectable({ providedIn: 'root' })
export class ItemsService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/items`;

  list(q?: string, cat?: number) {
    let params = new HttpParams();
    if (q) params = params.set('q', q);
    if (cat != null) params = params.set('cat', cat);
    return this.http.get<ItemDto[]>(this.base, { params });
  }
  get(id: string) { return this.http.get<ItemDto>(`${this.base}/${id}`); }
  create(req: CreateItemRequest) { return this.http.post<ItemDto>(this.base, req); }
  update(id: string, req: UpdateItemRequest) { return this.http.put<void>(`${this.base}/${id}`, req); }
  delete(id: string) { return this.http.delete<void>(`${this.base}/${id}`); }

  uploadImage(itemId: string, file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ path: string }>(`${this.base}/${itemId}/images`, form);
  }
}
