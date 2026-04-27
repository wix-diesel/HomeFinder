export interface Shelf {
  id: string;
  roomId: string;
  name: string;
  description: string;
  createdAt: string;
  updatedAt: string;
}

export interface Room {
  id: string;
  name: string;
  description: string;
  createdAt: string;
  updatedAt: string;
  shelves: Shelf[];
}

export interface CreateRoomPayload {
  name: string;
  description: string;
}

export interface UpdateRoomPayload extends CreateRoomPayload {}

export interface CreateShelfPayload {
  name: string;
  description: string;
}

export interface UpdateShelfPayload extends CreateShelfPayload {}
