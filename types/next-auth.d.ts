export declare module "next-auth" {
  interface Session {
    user: {
      name: string;
      email: string;
      sub: string;
      id: string;
      token: string;
      role: string;
      iat: number;
      exp: number;
      jti: string;
    };
  }
}
