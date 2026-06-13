import { validateSignUp } from '@/lib/validation';

describe('validateSignUp', () => {
  it('returns no errors for valid sign up data', () => {
    const errors = validateSignUp({
      username: 'john_doe',
      password: 'StrongPass1!',
      firstName: 'John',
      middleName: 'Paul',
      lastName: 'Doe',
      email: 'john@example.com',
    });

    expect(Object.keys(errors)).toHaveLength(0);
  });

  it('flags invalid username and weak password', () => {
    const errors = validateSignUp({
      username: 'bad!',
      password: 'weak',
      firstName: 'John',
      lastName: 'Doe',
      email: 'invalid',
    });

    expect(errors.username).toBeDefined();
    expect(errors.password).toBeDefined();
    expect(errors.email).toBeDefined();
  });
});
