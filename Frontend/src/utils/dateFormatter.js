export const formatIST = (dateString) => {
  if (!dateString) return '';
  // Ensure the string ends with 'Z' so JS treats it as UTC instead of local
  const utcString = dateString.endsWith('Z') ? dateString : `${dateString}Z`;
  const date = new Date(utcString);
  
  return date.toLocaleString('en-IN', {
    timeZone: 'Asia/Kolkata',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
    hour12: true
  });
};
