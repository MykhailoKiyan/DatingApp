import { Photo } from "./photo";

export interface User {
  id: number;
  username: string;
  gender: string;
  age: number;
  knowAs: string;
  created: Date;
  lastActive: Date;
  introduction?: Date;
  lookingFor?: string;
  interests?: string;
  city: string;
  country: string;
  photoUrl: string;
  photos: Photo[];
}
