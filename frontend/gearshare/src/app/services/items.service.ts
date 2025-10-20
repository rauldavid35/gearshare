import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ItemDto, CreateItemRequest, UpdateItemRequest } from '../models/item.model';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class ItemsService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/items`;

  list(q?: string, cat?: number) {
    let params = new HttpParams();
    if (q) params = params.set('q', q);
    if (cat != null) params = params.set('cat', String(cat));
    return this.http.get<ItemDto[]>(this.base, { params });
  }

  get(id: string) { 
    return this.http.get<ItemDto>(`${this.base}/${id}`).pipe(
      map(item => {
        console.log('[ItemsService.get] Raw API response:', item);
        console.log('[ItemsService.get] Images from API:', item.images);
        console.log('[ItemsService.get] Images type:', typeof item.images);
        console.log('[ItemsService.get] Is Array?:', Array.isArray(item.images));
        if (item.images && item.images.length > 0) {
          console.log('[ItemsService.get] First image:', item.images[0]);
          console.log('[ItemsService.get] First image type:', typeof item.images[0]);
        }
        return item;
      })
    );
  }

  create(req: CreateItemRequest) { 
    return this.http.post<ItemDto>(this.base, req); 
  }

  update(id: string, req: UpdateItemRequest) { 
    return this.http.put<void>(`${this.base}/${id}`, req); 
  }

  delete(id: string) { 
    return this.http.delete<void>(`${this.base}/${id}`); 
  }

  uploadImage(itemId: string, file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ id: string; url: string }>(`${this.base}/${itemId}/images`, form);
  }

  deleteImageByUrl(itemId: string, url: string) {
    console.log('[ItemsService.deleteImageByUrl] Called with url:', url);
    console.log('[ItemsService.deleteImageByUrl] URL type:', typeof url);
    console.log('[ItemsService.deleteImageByUrl] URL constructor:', url?.constructor?.name);
    console.log('[ItemsService.deleteImageByUrl] URL stringified:', JSON.stringify(url));
    
    const params = new HttpParams().set('url', url);
    console.log('[ItemsService.deleteImageByUrl] HttpParams toString:', params.toString());
    
    return this.http.delete<void>(`${environment.apiUrl}/items/${itemId}/images`, { params });
  }
}