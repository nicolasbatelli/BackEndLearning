const usernameRegex = /^[a-zA-Z0-9_]{3,30}$/;
const lettersOnlyRegex = /^[a-zA-Z\s'-]+$/;
const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$/;

export interface SignUpFormValues {
  username: string;
  password: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  email: string;
}

export function validateSignUp(values: SignUpFormValues): Record<string, string> {
  const errors: Record<string, string> = {};

  if (!values.username || !usernameRegex.test(values.username)) {
    errors.username = 'Username must be 3-30 characters and contain only letters, numbers, and underscores.';
  }
  if (!values.password || !passwordRegex.test(values.password)) {
    errors.password = 'Password must be at least 8 characters with uppercase, lowercase, digit, and special character.';
  }
  if (!values.firstName || !lettersOnlyRegex.test(values.firstName)) {
    errors.firstName = 'First name may contain only letters.';
  }
  if (values.middleName && !lettersOnlyRegex.test(values.middleName)) {
    errors.middleName = 'Middle name may contain only letters.';
  }
  if (!values.lastName || !lettersOnlyRegex.test(values.lastName)) {
    errors.lastName = 'Last name may contain only letters.';
  }
  if (!values.email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(values.email)) {
    errors.email = 'Enter a valid email address.';
  }

  return errors;
}

export function validateSignIn(usernameOrEmail: string, password: string): Record<string, string> {
  const errors: Record<string, string> = {};
  if (!usernameOrEmail) errors.usernameOrEmail = 'Username or email is required.';
  if (!password) errors.password = 'Password is required.';
  return errors;
}
